package com.quickapp.customer.controller;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.Map;

@RestController
@RequestMapping("/api/customer")
public class CustomerController {

    private static final Logger log = LoggerFactory.getLogger(CustomerController.class);

    @GetMapping
    public ResponseEntity<Map<String, String>> getAll() {
        // TODO: Implement — migrate logic from monolith's CustomerController
        return ResponseEntity.ok(Map.of("service", "Customer", "status", "scaffold"));
    }

    @GetMapping("/{id}")
    public ResponseEntity<Map<String, Object>> getById(@PathVariable int id) {
        // TODO: Implement — migrate logic from monolith
        return ResponseEntity.ok(Map.of("service", "Customer", "id", id));
    }
}
