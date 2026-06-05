package com.quickapp.notification.service;

import com.quickapp.notification.domain.entity.OrderNotification;
import com.quickapp.notification.domain.enums.NotificationStatus;
import com.quickapp.notification.domain.enums.NotificationType;
import com.quickapp.notification.domain.repository.NotificationRepository;
import com.quickapp.shared.contracts.events.OrderPlacedEvent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;

import java.time.Instant;
import java.util.UUID;

/**
 * Consumes OrderPlacedEvent messages from the Order service (via RabbitMQ)
 * and creates notification records. In the current local-testing configuration,
 * this is also exposed as an HTTP endpoint for synchronous event ingestion.
 */
@Service
public class OrderEventConsumer {

    private static final Logger log = LoggerFactory.getLogger(OrderEventConsumer.class);

    private final NotificationRepository repository;
    private final NotificationRenderer renderer;

    public OrderEventConsumer(NotificationRepository repository,
                              NotificationRenderer renderer) {
        this.repository = repository;
        this.renderer = renderer;
    }

    public OrderNotification handleOrderPlaced(OrderPlacedEvent orderEvent) {
        log.info("Processing OrderPlacedEvent for Order {}, Customer {}, Amount {}",
                orderEvent.orderId(), orderEvent.customerId(), orderEvent.totalAmount());

        OrderNotification notification = new OrderNotification();
        notification.setId(UUID.randomUUID());
        notification.setOrderId(orderEvent.orderId());
        notification.setCustomerId(orderEvent.customerId());
        notification.setOrderTotal(orderEvent.totalAmount());
        notification.setCustomerEmail("customer@example.com");
        notification.setCustomerName("Valued Customer");
        notification.setType(NotificationType.ORDER_CONFIRMATION);
        notification.setStatus(NotificationStatus.PENDING);
        notification.setCreatedAt(Instant.now());

        NotificationRenderer.RenderedNotification rendered =
                renderer.renderNotification(notification);
        notification.setRenderedSubject(rendered.subject());
        notification.setRenderedBody(rendered.body());
        notification.setStatus(NotificationStatus.RENDERED);

        notification = repository.save(notification);

        log.info("Notification {} rendered for Order {}",
                notification.getId(), notification.getOrderId());

        return notification;
    }
}
