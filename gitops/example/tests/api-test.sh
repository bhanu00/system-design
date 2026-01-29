#!/bin/bash

# API Testing Script for Products API
# This script tests all the API endpoints to ensure they work correctly

set -e

# Configuration
BASE_URL="${BASE_URL:-http://localhost:8080}"
VERBOSE="${VERBOSE:-false}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Helper functions
log() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

verbose() {
    if [ "$VERBOSE" = "true" ]; then
        echo -e "${YELLOW}[DEBUG]${NC} $1"
    fi
}

# Test function
test_endpoint() {
    local method=$1
    local endpoint=$2
    local expected_status=$3
    local data=$4
    local description=$5

    verbose "Testing: $method $endpoint"
    
    if [ -n "$data" ]; then
        response=$(curl -s -w "\n%{http_code}" -X "$method" \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$BASE_URL$endpoint")
    else
        response=$(curl -s -w "\n%{http_code}" -X "$method" "$BASE_URL$endpoint")
    fi

    # Extract status code (last line)
    status_code=$(echo "$response" | tail -n1)
    # Extract body (all but last line)
    body=$(echo "$response" | head -n -1)

    if [ "$status_code" -eq "$expected_status" ]; then
        log "✓ $description (Status: $status_code)"
        verbose "Response: $body"
        echo "$body"
    else
        error "✗ $description (Expected: $expected_status, Got: $status_code)"
        verbose "Response: $body"
        return 1
    fi
}

# Main test execution
main() {
    log "Starting API tests for Products API"
    log "Base URL: $BASE_URL"
    echo

    # Test 1: Health check
    log "=== Health Checks ==="
    test_endpoint "GET" "/health" 200 "" "Health check endpoint"
    test_endpoint "GET" "/ready" 200 "" "Readiness check endpoint"
    test_endpoint "GET" "/live" 200 "" "Liveness check endpoint"
    echo

    # Test 2: Get all products (should work even if empty)
    log "=== Product Operations ==="
    test_endpoint "GET" "/api/products" 200 "" "Get all products"
    echo

    # Test 3: Create a new product
    log "Creating a new product..."
    product_data='{"name":"Test Product","description":"A test product","price":29.99,"category":"Test"}'
    created_product=$(test_endpoint "POST" "/api/products" 201 "$product_data" "Create new product")
    
    # Extract product ID from response
    product_id=$(echo "$created_product" | grep -o '"id":[0-9]*' | cut -d':' -f2)
    
    if [ -n "$product_id" ]; then
        log "Created product with ID: $product_id"
        echo

        # Test 4: Get specific product
        test_endpoint "GET" "/api/products/$product_id" 200 "" "Get product by ID"
        echo

        # Test 5: Update product
        log "Updating product..."
        update_data='{"name":"Updated Test Product","description":"An updated test product","price":39.99,"category":"Updated"}'
        test_endpoint "PUT" "/api/products/$product_id" 200 "$update_data" "Update product"
        echo

        # Test 6: Get products by category
        test_endpoint "GET" "/api/products/category/Updated" 200 "" "Get products by category"
        echo

        # Test 7: Get product count
        test_endpoint "GET" "/api/products/count" 200 "" "Get product count"
        echo

        # Test 8: Delete product
        log "Cleaning up - deleting test product..."
        test_endpoint "DELETE" "/api/products/$product_id" 204 "" "Delete product"
        echo

        # Test 9: Verify product is deleted
        log "Verifying product deletion..."
        if ! test_endpoint "GET" "/api/products/$product_id" 404 "" "Verify product deleted" 2>/dev/null; then
            log "✓ Product successfully deleted (404 as expected)"
        fi
    else
        error "Failed to extract product ID from creation response"
        return 1
    fi

    echo
    log "=== Error Handling Tests ==="
    
    # Test invalid product ID
    test_endpoint "GET" "/api/products/99999" 404 "" "Get non-existent product" || true
    
    # Test invalid JSON
    invalid_json='{"name":"Invalid"'
    test_endpoint "POST" "/api/products" 400 "$invalid_json" "Create product with invalid JSON" || true
    
    echo
    log "All API tests completed successfully! ✓"
}

# Check if curl is available
if ! command -v curl &> /dev/null; then
    error "curl is required but not installed. Please install curl and try again."
    exit 1
fi

# Run tests
main "$@"