
# Testing Notes

## 1. Test Results
- Total tests: 20
- Passed: 20
- Failed: 0

## 2. Line Coverage
- Overall: 90.5%
- Application: 95.8%
- Domain: 100%

## 3. Test Data Libraries
- Moq: Used to mock dependencies and verify service interactions.
- AutoFixture: Used to generate valid test orders.
- Bogus: Used to generate realistic customer data with a fixed seed.

## 4. Discount Fairness
No, it is not entirely fair because a customer paying 999.99 EGP receives no discount, while a customer paying 1,000 EGP receives a 100 EGP discount.

## 5. Bugs Found
None.