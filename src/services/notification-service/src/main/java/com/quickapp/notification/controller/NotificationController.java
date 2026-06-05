package com.quickapp.notification.controller;

import com.quickapp.notification.domain.entity.OrderNotification;
import com.quickapp.notification.domain.repository.NotificationRepository;
import com.quickapp.notification.dto.OrderPlacedEventDto;
import com.quickapp.notification.service.OrderEventConsumer;
import com.quickapp.shared.contracts.events.OrderPlacedEvent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.data.domain.PageRequest;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.net.URI;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/notification")
public class NotificationController {

    private static final Logger log = LoggerFactory.getLogger(NotificationController.class);

    private final NotificationRepository repository;
    private final OrderEventConsumer eventConsumer;

    public NotificationController(NotificationRepository repository,
                                  OrderEventConsumer eventConsumer) {
        this.repository = repository;
        this.eventConsumer = eventConsumer;
    }

    @GetMapping
    public ResponseEntity<?> getAll(@RequestParam(defaultValue = "1") int page,
                                    @RequestParam(defaultValue = "20") int pageSize) {
        var notifications = repository.findAllByOrderByCreatedAtDesc(
                PageRequest.of(page - 1, pageSize));
        var result = notifications.getContent().stream()
                .map(n -> {
                    Map<String, Object> map = new LinkedHashMap<>();
                    map.put("id", n.getId());
                    map.put("orderId", n.getOrderId());
                    map.put("customerId", n.getCustomerId());
                    map.put("orderTotal", n.getOrderTotal());
                    map.put("type", n.getType());
                    map.put("status", n.getStatus());
                    map.put("renderedSubject", n.getRenderedSubject());
                    map.put("createdAt", n.getCreatedAt());
                    map.put("sentAt", n.getSentAt());
                    return map;
                })
                .toList();
        return ResponseEntity.ok(result);
    }

    @GetMapping("/{id}")
    public ResponseEntity<?> getById(@PathVariable UUID id) {
        return repository.findById(id)
                .map(n -> {
                    Map<String, Object> map = new LinkedHashMap<>();
                    map.put("id", n.getId());
                    map.put("orderId", n.getOrderId());
                    map.put("customerId", n.getCustomerId());
                    map.put("orderTotal", n.getOrderTotal());
                    map.put("customerEmail", n.getCustomerEmail());
                    map.put("customerName", n.getCustomerName());
                    map.put("type", n.getType());
                    map.put("status", n.getStatus());
                    map.put("renderedSubject", n.getRenderedSubject());
                    map.put("createdAt", n.getCreatedAt());
                    map.put("sentAt", n.getSentAt());
                    return ResponseEntity.ok((Object) map);
                })
                .orElse(ResponseEntity.notFound().build());
    }

    /**
     * Returns the rendered HTML email preview for a notification.
     */
    @GetMapping(value = "/{id}/preview", produces = MediaType.TEXT_HTML_VALUE)
    public ResponseEntity<String> getPreview(@PathVariable UUID id) {
        return repository.findById(id)
                .map(notification -> {
                    if (notification.getRenderedBody() == null || notification.getRenderedBody().isBlank()) {
                        return ResponseEntity.notFound().<String>build();
                    }
                    return ResponseEntity.ok()
                            .contentType(MediaType.TEXT_HTML)
                            .body(notification.getRenderedBody());
                })
                .orElse(ResponseEntity.notFound().build());
    }

    /**
     * HTTP endpoint for receiving OrderPlacedEvent messages.
     * In production this would be a RabbitMQ consumer; this endpoint
     * enables local testing without a message broker.
     */
    @PostMapping("events/order-placed")
    public ResponseEntity<?> receiveOrderPlacedEvent(@RequestBody OrderPlacedEventDto dto) {
        OrderPlacedEvent orderEvent = new OrderPlacedEvent(
                dto.orderId(),
                dto.customerId(),
                dto.totalAmount(),
                dto.placedAt());

        OrderNotification notification = eventConsumer.handleOrderPlaced(orderEvent);

        Map<String, Object> body = new LinkedHashMap<>();
        body.put("id", notification.getId());
        body.put("previewUrl", "/api/notification/" + notification.getId() + "/preview");

        return ResponseEntity.created(
                        URI.create("/api/notification/" + notification.getId() + "/preview"))
                .body(body);
    }
}
