public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
}
public class Order
{
    public int Id { get; set; }
    public int SenderId { get; set; }     // 寄件人
    public int ReceiverId { get; set; }   // 收件人
    public DateTime CreatedAt { get; set; }
    public int TotalAmount { get; set; }  // 先用 int 就好
    // 之後會加 OrderStatus
}
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ProductName { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
}
