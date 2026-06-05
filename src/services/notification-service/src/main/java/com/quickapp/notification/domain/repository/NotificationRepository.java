package com.quickapp.notification.domain.repository;

import com.quickapp.notification.domain.entity.OrderNotification;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.UUID;

@Repository
public interface NotificationRepository extends JpaRepository<OrderNotification, UUID> {

    List<OrderNotification> findByOrderIdOrderByCreatedAtDesc(UUID orderId);

    Page<OrderNotification> findAllByOrderByCreatedAtDesc(Pageable pageable);
}
