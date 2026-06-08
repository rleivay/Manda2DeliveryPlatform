using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.CheckOut
{
    public class CreateOrderGroupResult
    {
        public bool IsSuccess { get; set; }
        public int OrderGroupId { get; set; }
        public string? ErrorMessage { get; set; }

        // Resumen financiero incluido (opcional)
        public decimal TotalAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal DeliveryFee { get; set; }
        public int SubOrderCount { get; set; }

        public static CreateOrderGroupResult Success(int id) =>
            new() { IsSuccess = true, OrderGroupId = id };

        public static CreateOrderGroupResult Success(
            int id, decimal totalAmount, decimal serviceFee, decimal deliveryFee, int subOrderCount) =>
            new()
            {
                IsSuccess = true,
                OrderGroupId = id,
                TotalAmount = totalAmount,
                ServiceFee = serviceFee,
                DeliveryFee = deliveryFee,
                SubOrderCount = subOrderCount
            };

        public static CreateOrderGroupResult Failure(string msg) =>
            new() { IsSuccess = false, ErrorMessage = msg };
    }
}
