using MBKC.Repository.GrabFoods.Models;
using MBKC.Repository.Infrastructures;
using MBKC.Repository.Models;
using MBKC.Service.DTOs.Orders;
using MBKC.Service.Services.Interfaces;
using MBKC.Service.Utils;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private UnitOfWork _unitOfWork;
        public OrderService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<GetOrdersFromGrabFood> GetOrdersFromGrabFoodAsync(GrabFoodAuthenticationResponse grabFoodAuthentication, Store store, StorePartner storePartner)
        {
            try
            {
                List<GrabFoodOrderDetailResponse> grabFoodOrderDetails = new List<GrabFoodOrderDetailResponse>();
                Log.Information("Processing in OrderService to get Order's Ids.");
                List<string> upcomingOrderIds = await this._unitOfWork.GrabFoodRepository.GetUpcomingOrderIdsAsync(grabFoodAuthentication);
                Log.Information("Getting Upcoming Order's Ids Successfully in OrderService. => Data: {Data}", JsonConvert.SerializeObject(upcomingOrderIds));
                if (upcomingOrderIds is not null && upcomingOrderIds.Count > 0)
                {
                    foreach (var orderId in upcomingOrderIds)
                    {
                        GrabFoodOrderDetailResponse grabFoodOrderDetail = await this._unitOfWork.GrabFoodRepository.GetOrderAsync(grabFoodAuthentication, orderId);
                        grabFoodOrderDetail.Order.Status = "Upcoming";
                        grabFoodOrderDetails.Add(grabFoodOrderDetail);
                    }
                }

                List<string> preparingOrderIds = await this._unitOfWork.GrabFoodRepository.GetPreparingOrderIdsAsync(grabFoodAuthentication);
                Log.Information("Getting Preparing Order's Ids Successfully in OrderService. => Data: {Data}", JsonConvert.SerializeObject(preparingOrderIds));
                if (preparingOrderIds is not null && preparingOrderIds.Count > 0)
                {
                    foreach (var orderId in preparingOrderIds)
                    {
                        GrabFoodOrderDetailResponse grabFoodOrderDetail = await this._unitOfWork.GrabFoodRepository.GetOrderAsync(grabFoodAuthentication, orderId);
                        if (grabFoodOrderDetails.Any(x => x.Order.OrderId.Equals(grabFoodOrderDetail.Order.OrderId)) == false)
                        {
                            grabFoodOrderDetail.Order.Status = "Preparing";
                            grabFoodOrderDetails.Add(grabFoodOrderDetail);
                        }
                    }
                }
                Log.Information("Getting Order. Data: {Order}", JsonConvert.SerializeObject(grabFoodOrderDetails));
                List<FailedGrabFoodOrderDetail> failedOrders = new List<FailedGrabFoodOrderDetail>();
                List<Order> orders = new List<Order>();
                if (grabFoodOrderDetails is not null && grabFoodOrderDetails.Count > 0)
                {
                    foreach (var grabFoodOrder in grabFoodOrderDetails)
                    {
                        List<OrderDetail> orderDetails = new List<OrderDetail>();
                        bool isFailedOrder = false;
                        string reason = "";
                        decimal totalDiscountItems = 0;
                        foreach (var grabFoodItem in grabFoodOrder.Order.ItemInfo.Items)
                        {
                            if (storePartner.PartnerProducts.Any(x => x.ProductCode.ToLower().Equals(grabFoodItem.ItemId.ToLower())))
                            {
                                decimal discountPriceItem = 0;
                                if (grabFoodItem.DiscountInfo is not null && grabFoodItem.DiscountInfo.Count > 0)
                                {
                                    foreach (var discount in grabFoodItem.DiscountInfo)
                                    {
                                        discountPriceItem += discount.ItemDiscountPriceDisplay == "" ? 0 : decimal.Parse(discount.ItemDiscountPriceDisplay);
                                    }
                                    totalDiscountItems += discountPriceItem;
                                }
                                isFailedOrder = false;
                                PartnerProduct partnerProduct = storePartner.PartnerProducts.FirstOrDefault(x => x.ProductCode.ToLower().Equals(grabFoodItem.ItemId.ToLower()));
                                Log.Information("Partner Product in OrderService: {Data}", partnerProduct);
                                OrderDetail newOrderDetail = null;
                                int? parentProductId = null;
                                if (partnerProduct is not null && partnerProduct.Product.Type.ToLower().Equals("single"))
                                {
                                    newOrderDetail = new OrderDetail()
                                    {
                                        ProductId = partnerProduct.ProductId,
                                        SellingPrice = partnerProduct.Price,
                                        Note = grabFoodItem.Comment,
                                        Quantity = grabFoodItem.Quantity,
                                        DiscountPrice = discountPriceItem,
                                        ExtraOrderDetails = new List<OrderDetail>()
                                    };
                                }
                                
                                if (partnerProduct is not null && partnerProduct.Product.Type.ToLower().Equals("parent"))
                                {
                                    parentProductId = partnerProduct.Product.ProductId;
                                }

                                foreach (var modifierGroup in grabFoodItem.ModifierGroups)
                                {
                                    foreach (var modifier in modifierGroup.Modifiers)
                                    {
                                        if (storePartner.PartnerProducts.Any(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower())))
                                        {
                                            isFailedOrder = false;
                                            PartnerProduct partnerProductInModifier = null;
                                            if (storePartner.PartnerProducts.Where(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower())).Count() > 1 && parentProductId is not null)
                                            {
                                                partnerProductInModifier = storePartner.PartnerProducts.FirstOrDefault(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower()) && x.Product.ParentProductId == parentProductId);
                                            } else if(storePartner.PartnerProducts.Where(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower())).Count() == 1 && parentProductId is null)
                                            {
                                                partnerProductInModifier = storePartner.PartnerProducts.FirstOrDefault(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower()));
                                            }

                                            if (partnerProductInModifier is not null && partnerProductInModifier.Product.Type.ToLower().Equals("child"))
                                            {
                                                newOrderDetail = new OrderDetail()
                                                {
                                                    ProductId = partnerProductInModifier.ProductId,
                                                    SellingPrice = partnerProductInModifier.Price,
                                                    Note = grabFoodItem.Comment,
                                                    Quantity = grabFoodItem.Quantity,
                                                    DiscountPrice = discountPriceItem,
                                                    ExtraOrderDetails = new List<OrderDetail>()
                                                };
                                            }

                                            if (partnerProductInModifier is not null && partnerProductInModifier.Product.Type.ToLower().Equals("extra"))
                                            {
                                                OrderDetail newOrderDetailWithTypeExtra = new OrderDetail()
                                                {
                                                    ProductId = partnerProductInModifier.ProductId,
                                                    SellingPrice = partnerProductInModifier.Price,
                                                    Note = "",
                                                    DiscountPrice = 0,
                                                    Quantity = modifier.Quantity
                                                };
                                                newOrderDetail.ExtraOrderDetails.Add(newOrderDetailWithTypeExtra);
                                            }
                                        }
                                        else
                                        {
                                            reason = "There are a few products in the order that cannot be mapped to any products in the system.";
                                            isFailedOrder = true;
                                            break;
                                        }
                                    }
                                    if (isFailedOrder)
                                    {
                                        break;
                                    }
                                }
                                if (isFailedOrder == false)
                                {
                                    orderDetails.Add(newOrderDetail);
                                }
                            }
                            else
                            {
                                reason = "There are a few products in the order that cannot be mapped to any products in the system.";
                                isFailedOrder = true;
                                break;
                            }
                        }
                        if (isFailedOrder)
                        {
                            failedOrders.Add(new FailedGrabFoodOrderDetail()
                            {
                                OrderId = grabFoodOrder.Order.OrderId,
                                Reason = reason,
                            });
                        }
                        else
                        {
                            Log.Information("Processing Parse Order Data. Data: {Order}", JsonConvert.SerializeObject(orders));
                            Order newOrder = new Order()
                            {
                                OrderPartnerId = grabFoodOrder.Order.OrderId,
                                StoreId = store.StoreId,
                                PartnerId = storePartner.PartnerId,
                                OrderDetails = orderDetails,
                                CustomerName = grabFoodOrder.Order.Eater.Name,
                                CustomerPhone = StringUtil.ChangeNumberPhoneFromGrabFood(grabFoodOrder.Order.Eater.MobileNumber),
                                Address = grabFoodOrder.Order.Eater.Address.Address,
                                ShipperName = grabFoodOrder.Order.Driver.Name,
                                ShipperPhone = StringUtil.ChangeNumberPhoneFromGrabFood(grabFoodOrder.Order.Driver.MobileNumber),
                                PartnerOrderStatus = grabFoodOrder.Order.Status,
                                DisplayId = grabFoodOrder.Order.DisplayID,
                                DeliveryFee = grabFoodOrder.Order.Fare.DeliveryFeeDisplay == "" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.DeliveryFeeDisplay),
                                FinalTotalPrice = grabFoodOrder.Order.Fare.ReducedPriceDisplay == "" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.ReducedPriceDisplay),
                                SubTotalPrice = grabFoodOrder.Order.Fare.RevampedSubtotalDisplay == "" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.RevampedSubtotalDisplay),
                                TotalStoreDiscount = (grabFoodOrder.Order.Fare.TotalDiscountAmountDisplay == "" ? 0 : (decimal.Parse(grabFoodOrder.Order.Fare.TotalDiscountAmountDisplay)) - totalDiscountItems),
                                PromotionPrice = (grabFoodOrder.Order.Fare.PromotionDisplay == "" || grabFoodOrder.Order.Fare.PromotionDisplay == "-" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.PromotionDisplay)),
                                Tax = grabFoodOrder.Order.Fare.TaxDisplay == "" ? 0 : float.Parse(grabFoodOrder.Order.Fare.TaxDisplay),
                                TaxPartnerCommission = storePartner.Partner.TaxCommission,
                                Cutlery = grabFoodOrder.Order.Cutlery,
                                Note = grabFoodOrder.Order.Eater.Comment,
                                PaymentMethod = grabFoodOrder.Order.PaymentMethod,
                                StorePartnerCommission = storePartner.Commission
                            };
                            orders.Add(newOrder);
                        }
                    }
                }

                Log.Information("Getting Order. Data: {Order}", JsonConvert.SerializeObject(orders));
                Log.Information("Getting Failed Order. Data: {Order}", JsonConvert.SerializeObject(failedOrders));

                return new GetOrdersFromGrabFood()
                {
                    Orders = orders,
                    FailedOrders = failedOrders
                };
            }
            catch (Exception ex)
            {
                Log.Error("Error in OrderService => Exception: {Message}", ex.Message);
                return null;
            }
        }


        public async Task<GetOrdersFromGrabFood> GetOrdersFromGrabFoodAsync(List<GrabFoodOrderDetailResponse> grabFoodOrderDetails, Store store, StorePartner storePartner)
        {
            try
            {
                List<FailedGrabFoodOrderDetail> failedOrders = new List<FailedGrabFoodOrderDetail>();
                List<Order> orders = new List<Order>();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Start parse orders.");
                Console.ResetColor();
                if (grabFoodOrderDetails is not null && grabFoodOrderDetails.Count > 0)
                {
                    foreach (var grabFoodOrder in grabFoodOrderDetails)
                    {
                        List<OrderDetail> orderDetails = new List<OrderDetail>();
                        bool isFailedOrder = false;
                        string reason = "";
                        decimal totalDiscountItems = 0;
                        foreach (var grabFoodItem in grabFoodOrder.Order.ItemInfo.Items)
                        {
                            if (storePartner.PartnerProducts.Any(x => x.ProductCode.ToLower().Equals(grabFoodItem.ItemId.ToLower())))
                            {
                                decimal discountPriceItem = 0;
                                if (grabFoodItem.DiscountInfo is not null && grabFoodItem.DiscountInfo.Count > 0)
                                {
                                    foreach (var discount in grabFoodItem.DiscountInfo)
                                    {
                                        discountPriceItem += discount.ItemDiscountPriceDisplay == "" ? 0 : decimal.Parse(discount.ItemDiscountPriceDisplay);
                                    }
                                    totalDiscountItems += discountPriceItem;
                                }
                                isFailedOrder = false;
                                PartnerProduct partnerProduct = storePartner.PartnerProducts.FirstOrDefault(x => x.ProductCode.ToLower().Equals(grabFoodItem.ItemId.ToLower()));
                                Log.Information("Partner Product in OrderService: {Data}", partnerProduct);
                                OrderDetail newOrderDetail = null;
                                int? parentProductId = null;
                                if (partnerProduct is not null && partnerProduct.Product.Type.ToLower().Equals("single"))
                                {
                                    newOrderDetail = new OrderDetail()
                                    {
                                        ProductId = partnerProduct.ProductId,
                                        SellingPrice = partnerProduct.Price,
                                        Note = grabFoodItem.Comment,
                                        Quantity = grabFoodItem.Quantity,
                                        DiscountPrice = discountPriceItem,
                                        ExtraOrderDetails = new List<OrderDetail>()
                                    };
                                }

                                if (partnerProduct is not null && partnerProduct.Product.Type.ToLower().Equals("parent"))
                                {
                                    parentProductId = partnerProduct.Product.ProductId;
                                }

                                foreach (var modifierGroup in grabFoodItem.ModifierGroups)
                                {
                                    foreach (var modifier in modifierGroup.Modifiers)
                                    {
                                        if (storePartner.PartnerProducts.Any(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower())))
                                        {
                                            isFailedOrder = false;
                                            PartnerProduct partnerProductInModifier = null;
                                            if (storePartner.PartnerProducts.Where(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower())).Count() >= 1 && parentProductId is not null)
                                            {
                                                partnerProductInModifier = storePartner.PartnerProducts.FirstOrDefault(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower()) && x.Product.ParentProductId == parentProductId);
                                            }
                                            else if (storePartner.PartnerProducts.Where(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower())).Count() == 1 && parentProductId is null)
                                            {
                                                partnerProductInModifier = storePartner.PartnerProducts.FirstOrDefault(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower()));
                                            }

                                            if (partnerProductInModifier is not null && partnerProductInModifier.Product.Type.ToLower().Equals("child"))
                                            {
                                                newOrderDetail = new OrderDetail()
                                                {
                                                    ProductId = partnerProductInModifier.ProductId,
                                                    SellingPrice = partnerProductInModifier.Price,
                                                    Note = grabFoodItem.Comment,
                                                    Quantity = grabFoodItem.Quantity,
                                                    DiscountPrice = discountPriceItem,
                                                    ExtraOrderDetails = new List<OrderDetail>()
                                                };
                                            }

                                            if (partnerProductInModifier is not null && partnerProductInModifier.Product.Type.ToLower().Equals("extra"))
                                            {
                                                OrderDetail newOrderDetailWithTypeExtra = new OrderDetail()
                                                {
                                                    ProductId = partnerProductInModifier.ProductId,
                                                    SellingPrice = partnerProductInModifier.Price,
                                                    Note = "",
                                                    DiscountPrice = 0,
                                                    Quantity = modifier.Quantity
                                                };
                                                newOrderDetail.ExtraOrderDetails.Add(newOrderDetailWithTypeExtra);
                                            }
                                        }
                                        else
                                        {
                                            reason = "There are a few products in the order that cannot be mapped to any products in the system.";
                                            isFailedOrder = true;
                                            break;
                                        }
                                    }
                                    if (isFailedOrder)
                                    {
                                        break;
                                    }
                                }
                                if (isFailedOrder == false)
                                {
                                    orderDetails.Add(newOrderDetail);
                                }
                            }
                            else
                            {
                                reason = "There are a few products in the order that cannot be mapped to any products in the system.";
                                isFailedOrder = true;
                                break;
                            }
                        }
                        if (isFailedOrder)
                        {
                            failedOrders.Add(new FailedGrabFoodOrderDetail()
                            {
                                OrderId = grabFoodOrder.Order.OrderId,
                                Reason = reason,
                            });
                        }
                        else
                        {
                            Log.Information("Processing Parse Order Data. Data: {Order}", JsonConvert.SerializeObject(orders));
                            Order newOrder = new Order()
                            {
                                OrderPartnerId = grabFoodOrder.Order.OrderId,
                                StoreId = store.StoreId,
                                PartnerId = storePartner.PartnerId,
                                OrderDetails = orderDetails,
                                CustomerName = grabFoodOrder.Order.Eater.Name,
                                CustomerPhone = StringUtil.ChangeNumberPhoneFromGrabFood(grabFoodOrder.Order.Eater.MobileNumber),
                                Address = grabFoodOrder.Order.Eater.Address.Address,
                                ShipperName = grabFoodOrder.Order.Driver.Name,
                                ShipperPhone = StringUtil.ChangeNumberPhoneFromGrabFood(grabFoodOrder.Order.Driver.MobileNumber),
                                PartnerOrderStatus = grabFoodOrder.Order.Status,
                                DisplayId = grabFoodOrder.Order.DisplayID,
                                DeliveryFee = grabFoodOrder.Order.Fare.DeliveryFeeDisplay == "" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.DeliveryFeeDisplay),
                                FinalTotalPrice = grabFoodOrder.Order.Fare.ReducedPriceDisplay == "" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.ReducedPriceDisplay),
                                SubTotalPrice = grabFoodOrder.Order.Fare.RevampedSubtotalDisplay == "" ? 0 : (decimal.Parse(grabFoodOrder.Order.Fare.RevampedSubtotalDisplay) - totalDiscountItems),
                                TotalStoreDiscount = (grabFoodOrder.Order.Fare.TotalDiscountAmountDisplay == "" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.TotalDiscountAmountDisplay)),
                                PromotionPrice = (grabFoodOrder.Order.Fare.PromotionDisplay == "" || grabFoodOrder.Order.Fare.PromotionDisplay == "-" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.PromotionDisplay)),
                                Tax = grabFoodOrder.Order.Fare.TaxDisplay == "" ? 0 : float.Parse(grabFoodOrder.Order.Fare.TaxDisplay),
                                TaxPartnerCommission = storePartner.Partner.TaxCommission,
                                Cutlery = grabFoodOrder.Order.Cutlery,
                                Note = grabFoodOrder.Order.Eater.Comment,
                                PaymentMethod = grabFoodOrder.Order.PaymentMethod,
                                StorePartnerCommission = storePartner.Commission
                            };
                            orders.Add(newOrder);
                        }
                    }
                }

                Log.Information("Getting Order. Data: {Order}", JsonConvert.SerializeObject(orders));
                Log.Information("Getting Failed Order. Data: {Order}", JsonConvert.SerializeObject(failedOrders));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Parse orders Successfully.");
                Console.ResetColor();

                return new GetOrdersFromGrabFood()
                {
                    Orders = orders,
                    FailedOrders = failedOrders
                };
            }
            catch (Exception ex)
            {
                Log.Error("Error in OrderService => Exception: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Parse orders Failed.");
                Console.ResetColor();
                return null;
            }
        }

        public async Task<GetOrdersFromGrabFood> GetOrdersFromGrabFoodAsync_Tool(List<GrabFoodOrderDetailResponse> grabFoodOrderDetails, Store store, StorePartner storePartner)
        {
            try
            {
                List<FailedGrabFoodOrderDetail> failedOrders = new List<FailedGrabFoodOrderDetail>();
                List<Order> orders = new List<Order>();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Start parse orders.");
                Console.ResetColor();
                if (grabFoodOrderDetails is not null && grabFoodOrderDetails.Count > 0)
                {
                    foreach (var grabFoodOrder in grabFoodOrderDetails)
                    {
                        List<OrderDetail> orderDetails = new List<OrderDetail>();
                        bool isFailedOrder = false;
                        string reason = "";
                        decimal totalDiscountItems = 0;
                        foreach (var grabFoodItem in grabFoodOrder.Order.ItemInfo.Items)
                        {
                            if (storePartner.PartnerProducts.Any(x => x.ProductCode.ToLower().Equals(grabFoodItem.ItemId.ToLower())))
                            {
                                decimal discountPriceItem = 0;
                                if(grabFoodItem.DiscountInfo is not null && grabFoodItem.DiscountInfo.Count > 0)
                                {
                                    foreach (var discount in grabFoodItem.DiscountInfo)
                                    {
                                        discountPriceItem += discount.ItemDiscountPriceDisplay == "" ? 0 : decimal.Parse(discount.ItemDiscountPriceDisplay);
                                    }
                                    totalDiscountItems += discountPriceItem;
                                }
                                isFailedOrder = false;
                                PartnerProduct partnerProduct = storePartner.PartnerProducts.FirstOrDefault(x => x.ProductCode.ToLower().Equals(grabFoodItem.ItemId.ToLower()));
                                Log.Information("Partner Product in OrderService: {Data}", partnerProduct);
                                OrderDetail newOrderDetail = null;
                                int? parentProductId = null;
                                if (partnerProduct is not null && partnerProduct.Product.Type.ToLower().Equals("single"))
                                {
                                    newOrderDetail = new OrderDetail()
                                    {
                                        ProductId = partnerProduct.ProductId,
                                        SellingPrice = partnerProduct.Price,
                                        Note = grabFoodItem.Comment,
                                        Quantity = grabFoodItem.Quantity,
                                        DiscountPrice = discountPriceItem,
                                        ExtraOrderDetails = new List<OrderDetail>()
                                    };
                                }

                                if (partnerProduct is not null && partnerProduct.Product.Type.ToLower().Equals("parent"))
                                {
                                    parentProductId = partnerProduct.Product.ProductId;
                                }

                                foreach (var modifierGroup in grabFoodItem.ModifierGroups)
                                {
                                    foreach (var modifier in modifierGroup.Modifiers)
                                    {
                                        if (storePartner.PartnerProducts.Any(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower())))
                                        {
                                            isFailedOrder = false;
                                            PartnerProduct partnerProductInModifier = null;
                                            if (storePartner.PartnerProducts.Where(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower())).Count() >= 1 && parentProductId is not null)
                                            {
                                                partnerProductInModifier = storePartner.PartnerProducts.FirstOrDefault(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower()) && x.Product.ParentProductId == parentProductId);
                                            }
                                            else if (storePartner.PartnerProducts.Where(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower())).Count() == 1 && parentProductId is null)
                                            {
                                                partnerProductInModifier = storePartner.PartnerProducts.FirstOrDefault(x => x.ProductCode.ToLower().Equals(modifier.ModifierId.ToLower()));
                                            }

                                            if (partnerProductInModifier is not null && partnerProductInModifier.Product.Type.ToLower().Equals("child"))
                                            {
                                                newOrderDetail = new OrderDetail()
                                                {
                                                    ProductId = partnerProductInModifier.ProductId,
                                                    SellingPrice = partnerProductInModifier.Price,
                                                    Note = grabFoodItem.Comment,
                                                    Quantity = grabFoodItem.Quantity,
                                                    DiscountPrice = discountPriceItem,
                                                    ExtraOrderDetails = new List<OrderDetail>()
                                                };
                                            }

                                            if (partnerProductInModifier is not null && partnerProductInModifier.Product.Type.ToLower().Equals("extra"))
                                            {
                                                OrderDetail newOrderDetailWithTypeExtra = new OrderDetail()
                                                {
                                                    ProductId = partnerProductInModifier.ProductId,
                                                    SellingPrice = partnerProductInModifier.Price,
                                                    Note = "",
                                                    DiscountPrice = 0,
                                                    Quantity = modifier.Quantity
                                                };
                                                newOrderDetail.ExtraOrderDetails.Add(newOrderDetailWithTypeExtra);
                                            }
                                        }
                                        else
                                        {
                                            reason = "There are a few products in the order that cannot be mapped to any products in the system.";
                                            isFailedOrder = true;
                                            break;
                                        }
                                    }
                                    if (isFailedOrder)
                                    {
                                        break;
                                    }
                                }
                                if (isFailedOrder == false)
                                {
                                    orderDetails.Add(newOrderDetail);
                                }
                            }
                            else
                            {
                                reason = "There are a few products in the order that cannot be mapped to any products in the system.";
                                isFailedOrder = true;
                                break;
                            }
                        }
                        if (isFailedOrder)
                        {
                            failedOrders.Add(new FailedGrabFoodOrderDetail()
                            {
                                OrderId = grabFoodOrder.Order.OrderId,
                                Reason = reason,
                            });
                        }
                        else
                        {
                            Log.Information("Processing Parse Order Data. Data: {Order}", JsonConvert.SerializeObject(orders));
                            Order newOrder = new Order()
                            {
                                OrderPartnerId = grabFoodOrder.Order.OrderId,
                                StoreId = store.StoreId,
                                PartnerId = storePartner.PartnerId,
                                OrderDetails = orderDetails,
                                CustomerName = grabFoodOrder.Order.Eater.Name,
                                CustomerPhone = StringUtil.ChangeNumberPhoneFromGrabFood(grabFoodOrder.Order.Eater.MobileNumber),
                                Address = grabFoodOrder.Order.Eater.Address.Address,
                                ShipperName = grabFoodOrder.Order.Driver.Name,
                                ShipperPhone = StringUtil.ChangeNumberPhoneFromGrabFood(grabFoodOrder.Order.Driver.MobileNumber),
                                PartnerOrderStatus = grabFoodOrder.Order.Status,
                                DisplayId = grabFoodOrder.Order.DisplayID,
                                DeliveryFee = grabFoodOrder.Order.Fare.DeliveryFeeDisplay == "" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.DeliveryFeeDisplay),
                                FinalTotalPrice = grabFoodOrder.Order.Fare.ReducedPriceDisplay == "" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.ReducedPriceDisplay),
                                SubTotalPrice = grabFoodOrder.Order.Fare.RevampedSubtotalDisplay == "" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.RevampedSubtotalDisplay),
                                TotalStoreDiscount = (grabFoodOrder.Order.Fare.TotalDiscountAmountDisplay == "" ? 0 : (decimal.Parse(grabFoodOrder.Order.Fare.TotalDiscountAmountDisplay)) - totalDiscountItems),
                                PromotionPrice = (grabFoodOrder.Order.Fare.PromotionDisplay == "" || grabFoodOrder.Order.Fare.PromotionDisplay == "-" ? 0 : decimal.Parse(grabFoodOrder.Order.Fare.PromotionDisplay)),
                                Tax = grabFoodOrder.Order.Fare.TaxDisplay == "" ? 0 : float.Parse(grabFoodOrder.Order.Fare.TaxDisplay),
                                TaxPartnerCommission = storePartner.Partner.TaxCommission,
                                Cutlery = grabFoodOrder.Order.Cutlery,
                                Note = grabFoodOrder.Order.Eater.Comment,
                                PaymentMethod = grabFoodOrder.Order.PaymentMethod,
                                StorePartnerCommission = storePartner.Commission
                            };
                            orders.Add(newOrder);
                        }
                    }
                }

                Log.Information("Getting Order. Data: {Order}", JsonConvert.SerializeObject(orders));
                Log.Information("Getting Failed Order. Data: {Order}", JsonConvert.SerializeObject(failedOrders));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Parse orders Successfully.");
                Console.ResetColor();

                return new GetOrdersFromGrabFood()
                {
                    Orders = orders,
                    FailedOrders = failedOrders
                };
            }
            catch (Exception ex)
            {
                Log.Error("Error in OrderService => Exception: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Parse orders Failed.");
                Console.ResetColor();
                throw new Exception(ex.Message);
            }
        }

        public async Task<Tuple<Order, bool>> GetOrderAsync(string partnerOrderId)
        {
            try
            {
                Log.Information("Processing in OrderService to get order.");
                Order order = await this._unitOfWork.OrderRepository.GetOrderAsync(partnerOrderId);
                Log.Information("Getting order successfully in OrderService. => Data: {Data}", JsonConvert.SerializeObject(order));
                return new Tuple<Order, bool>(order, true);
            } catch(Exception ex)
            {
                Log.Error("Error in OrderService => Exception: {Message}", ex.Message);
                return new Tuple<Order, bool>(null, false);
            }
        }
        
        public async Task<Tuple<Order, bool>> GetOrderAsync_Tool(string partnerOrderId)
        {
            try
            {
                Log.Information("Processing in OrderService to get order.");
                Order order = await this._unitOfWork.OrderRepository.GetOrderAsync(partnerOrderId);
                Log.Information("Getting order successfully in OrderService. => Data: {Data}", JsonConvert.SerializeObject(order));
                return new Tuple<Order, bool>(order, true);
            } catch(Exception ex)
            {
                Log.Error("Error in OrderService => Exception: {Message}", ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            Order createdOrder = null;
            try
            {
                Log.Information("Processing in OrderService to create new order.");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Start create Order.");
                Console.ResetColor();
                createdOrder = await this._unitOfWork.OrderRepository.CreateOrderAsync(order);
                Log.Information("Created order successfully in OrderService.");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Created Order Sucessfully.");
                Console.ResetColor();

            } catch(Exception ex)
            {
                Log.Error("Error in OrderService => Exception: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Created Order Failed.");
                Console.ResetColor();
            }
            return createdOrder;
        }
        
        public async Task<Order> CreateOrderAsync_Tool(Order order)
        {
            Order createdOrder = null;
            try
            {
                Log.Information("Processing in OrderService to create new order.");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Start create Order.");
                Console.ResetColor();
                createdOrder = await this._unitOfWork.OrderRepository.CreateOrderAsync(order);
                Log.Information("Created order successfully in OrderService.");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Created Order Sucessfully.");
                Console.ResetColor();
                return createdOrder;
            } catch(Exception ex)
            {
                Log.Error("Error in OrderService => Exception: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Created Order Failed.");
                Console.ResetColor();
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<Order> UpdateOrderAsync(Order order)
        {
            Order updatedOrder = null;
            try
            {
                Log.Information("Processing in OrderService to update new order.");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Start update order orders.");
                Console.ResetColor();
                updatedOrder = await this._unitOfWork.OrderRepository.UpdateOrderAsync(order);
                Log.Information("Updated order successfully in OrderService.");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Updated Order successfully.");
                Console.ResetColor();
            } catch(Exception ex)
            {
                Log.Error("Error in OrderService => Exception: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Updated Order Failed.");
                Console.ResetColor();
            }
            return updatedOrder;
        }
        
        public async Task<Order> UpdateOrderAsync_Tool(Order order)
        {
            Order updatedOrder = null;
            try
            {
                Log.Information("Processing in OrderService to update new order.");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Start update order orders.");
                Console.ResetColor();
                updatedOrder = await this._unitOfWork.OrderRepository.UpdateOrderAsync(order);
                Log.Information("Updated order successfully in OrderService.");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Updated Order successfully.");
                Console.ResetColor();
                return updatedOrder;
            } catch(Exception ex)
            {
                Log.Error("Error in OrderService => Exception: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Updated Order Failed.");
                Console.ResetColor();
                throw new Exception(ex.Message);
            }
        }
    }
}
