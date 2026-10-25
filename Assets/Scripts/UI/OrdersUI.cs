using System.Collections.Generic;
using NomadChef.Orders;
using UnityEngine;

namespace NomadChef.UI
{
    /// <summary>Row of order tickets along the top of the screen, oldest on the left.</summary>
    public class OrdersUI : MonoBehaviour
    {
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private Transform container;
        [SerializeField] private OrderTicketUI ticketTemplate;

        private readonly Dictionary<Order, OrderTicketUI> tickets = new Dictionary<Order, OrderTicketUI>();

        private void Awake()
        {
            ticketTemplate.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            orderManager.OrderAdded += AddTicket;
            orderManager.OrderCompleted += OnOrderCompleted;
            orderManager.OrderExpired += RemoveTicket;
        }

        private void OnDisable()
        {
            orderManager.OrderAdded -= AddTicket;
            orderManager.OrderCompleted -= OnOrderCompleted;
            orderManager.OrderExpired -= RemoveTicket;
        }

        private void AddTicket(Order order)
        {
            OrderTicketUI ticket = Instantiate(ticketTemplate, container);
            ticket.gameObject.SetActive(true);
            ticket.Bind(order);
            tickets.Add(order, ticket);
        }

        private void OnOrderCompleted(Order order, int points) => RemoveTicket(order);

        private void RemoveTicket(Order order)
        {
            if (!tickets.TryGetValue(order, out OrderTicketUI ticket))
                return;

            tickets.Remove(order);
            Destroy(ticket.gameObject);
        }
    }
}
