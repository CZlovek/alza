namespace AlzaShopApi;

public static class Constants
{
    public static class Test
    {
        public const int ProductsNo = 1000;
    }

    public static class RabbitMq
    {
        public const string Exchange = "Alza.Shop.Exchange";
        public const string RoutingKey = "Alza.Shop.Commands.Update";
    }

    public static class Paging
    {
        public const int PageSize = 10;
    }

    public static class Product
    {
        public const string Server = "https://www.example.com";
    }

    public static class Messages
    {
        public static class Errors
        {
            public static class Server
            {
                public const string InternalServerErrorDescription = "An error occurred while processing your request.";
                public const string InternalServerErrorTitle = "Messages Error";
            }
        }
    }
}