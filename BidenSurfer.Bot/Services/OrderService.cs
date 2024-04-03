using BidenSurfer.Infras;
using BidenSurfer.Infras.Models;

namespace BidenSurfer.Bot.Services
{
    public interface IOrderService
    {
        OrderDto GetById(Guid id);
        List<OrderDto> GetByUserId(Guid userId);
        List<OrderDto> GetAll();
        bool AddOrEdit(OrderDto order);
        bool AddFilledOrder(OrderDto order);
        bool Delete(Guid id);
        bool DeleteByOrderId(string id);
        bool DeleteRange(List<Guid> ids);
    }

    public class OrderService : IOrderService
    {
        private readonly IRedisCacheService _redisCacheService;

        public OrderService(IRedisCacheService redisCacheService)
        {
            _redisCacheService = redisCacheService;
        }

        public bool AddFilledOrder(OrderDto order)
        {
            var orders = _redisCacheService.GetCachedData<List<OrderDto>>(Constants.RedisFilledOrders);
            var entity = orders?.FirstOrDefault(c => c.Id == order.Id);
            if (entity == null)
            {
                //Add new                
                orders?.Add(order);
                _redisCacheService.SetCachedData(Constants.RedisFilledOrders, orders, TimeSpan.FromDays(3));
            }
            
            return true;
        }

        public bool AddOrEdit(OrderDto order)
        {
            var orders = _redisCacheService.GetCachedData<List<OrderDto>>(Constants.RedisAllOrders);
            var entity = orders?.FirstOrDefault(c => c.Id == order.Id);
            if (entity == null)
            {
                //Add new                
                orders?.Add(order);
            }
            else
            {
                //Edit
                entity.ExternalOrderId = order.ExternalOrderId;
                entity.Symbol = order.Symbol;
                entity.PositionSide = order.PositionSide;
                entity.Status = order.Status;
                entity.Amount = order.Amount;
                entity.UserId = order.UserId;
                entity.CreatedDate = order.CreatedDate;
                entity.TpPrice = order.TpPrice ?? 0;
            }

            _redisCacheService.SetCachedData(Constants.RedisAllOrders, orders, TimeSpan.FromDays(3));
            return true;
        }

        public bool Delete(Guid id)
        {
            var orders = _redisCacheService.GetCachedData<List<OrderDto>>(Constants.RedisAllOrders);
            var order = orders?.FirstOrDefault(c => c.Id == id);
            if (order == null)
            {
                return false;
            }
            var userId = order.UserId;
            orders?.Remove(order);
            
            _redisCacheService.SetCachedData(Constants.RedisAllOrders, orders, TimeSpan.FromDays(3));
            return true;
        }

        public bool DeleteByOrderId(string id)
        {
            var orders = _redisCacheService.GetCachedData<List<OrderDto>>(Constants.RedisAllOrders);
            var order = orders?.FirstOrDefault(c => c.ExternalOrderId == id);
            if (order == null)
            {
                return false;
            }
            var userId = order.UserId;
            orders?.Remove(order);

            _redisCacheService.SetCachedData(Constants.RedisAllOrders, orders, TimeSpan.FromDays(3));
            return true;
        }

        public bool DeleteRange(List<Guid> ids)
        {
            var orders = _redisCacheService.GetCachedData<List<OrderDto>>(Constants.RedisAllOrders);
            
            orders?.RemoveAll(c => ids.Contains(c.Id));

            _redisCacheService.SetCachedData(Constants.RedisAllOrders, orders, TimeSpan.FromDays(3));
            return true;
        }

        public List<OrderDto> GetAll()
        {

            List<OrderDto> resultDto = new List<OrderDto>();
            var cachedData = _redisCacheService.GetCachedData<List<OrderDto>>(Constants.RedisAllOrders);
            if (cachedData != null)
            {
                return cachedData;
            }
            
            return resultDto;
        }

        public OrderDto GetById(Guid id)
        {
            var orders = _redisCacheService.GetCachedData<List<OrderDto>>(Constants.RedisAllOrders);
            var order = orders?.FirstOrDefault(x => x.Id == id);
            if (null == order) return null;
            return order;
        }

        public List<OrderDto> GetByUserId(Guid userId)
        {
            var orders = _redisCacheService.GetCachedData<List<OrderDto>>(Constants.RedisAllOrders);
            var userOrders = orders?.Where(x => x.UserId == userId).ToList();
            return userOrders; 
        }
    }
}
