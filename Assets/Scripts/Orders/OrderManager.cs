using System;
using System.Collections.Generic;
using NomadChef.KitchenObjects;
using UnityEngine;

namespace NomadChef.Orders
{
    /// <summary>
    /// Keeps the queue of customer orders: spawns new ones over time, expires impatient ones and
    /// matches delivered plates. Only runs while enabled (the GameManager enables it during play).
    /// </summary>
    public class OrderManager : MonoBehaviour
    {
        [SerializeField] private DishRecipeSO[] menu;
        [SerializeField, Min(0.1f)] private float spawnInterval = 15f;
        [SerializeField, Min(0f)] private float emptyQueueDelay = 2f;
        [SerializeField, Min(1)] private int maxOrders = 4;
        [Tooltip("Extra points for a delivery, scaled by how much patience the customer had left.")]
        [SerializeField, Min(0)] private int maxTip = 10;

        private readonly List<Order> orders = new List<Order>();
        private float spawnTimer;

        public event Action<Order> OrderAdded;
        public event Action<Order, int> OrderCompleted;
        public event Action<Order> OrderExpired;
        public event Action DeliveryRejected;

        public IReadOnlyList<Order> Orders => orders;

        private void Update()
        {
            for (int i = orders.Count - 1; i >= 0; i--)
            {
                Order order = orders[i];
                order.TimeLeft -= Time.deltaTime;
                if (order.TimeLeft <= 0f)
                {
                    orders.RemoveAt(i);
                    OrderExpired?.Invoke(order);
                }
            }

            if (orders.Count >= maxOrders)
                return;

            // Never leave the cooks idle for long: an empty queue gets a new order quickly.
            if (orders.Count == 0)
                spawnTimer = Mathf.Max(spawnTimer, spawnInterval - emptyQueueDelay);

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                AddOrder(menu[UnityEngine.Random.Range(0, menu.Length)]);
            }
        }

        /// <summary>Serves the oldest order matching the plate. Returns false if nobody ordered it.</summary>
        public bool TryDeliver(PlateKitchenObject plate)
        {
            for (int i = 0; i < orders.Count; i++)
            {
                Order order = orders[i];
                if (!order.Recipe.Matches(plate.Ingredients))
                    continue;

                orders.RemoveAt(i);
                int points = order.Recipe.Points + Mathf.RoundToInt(maxTip * order.NormalizedTimeLeft);
                OrderCompleted?.Invoke(order, points);
                return true;
            }

            DeliveryRejected?.Invoke();
            return false;
        }

        private void AddOrder(DishRecipeSO recipe)
        {
            var order = new Order(recipe);
            orders.Add(order);
            OrderAdded?.Invoke(order);
        }
    }
}
