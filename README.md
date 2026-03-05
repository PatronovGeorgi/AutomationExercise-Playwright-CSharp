# AutomationExercise Test Automation Framework

E-commerce test automation framework for [automationexercise.com](https://automationexercise.com)

## 🛠️ Tech Stack
- **UI Testing:** Playwright + C# + NUnit
- **API Testing:** Postman
- **Pattern:** Page Object Model (POM)
- **Version Control:** Git + GitHub

## 📁 Project Structure
```
AutomationExerciseTest/   → UI test suite (Playwright/C#)
PostmanTests/             → API test collection (Postman)
```

## ✅ UI Tests (35 test cases)
- User Registration & Login
- Product Search & Filtering
- Shopping Cart Management
- Checkout & Payment Flow
- Contact Form & Subscriptions

## ✅ API Tests (13 test cases)
- Products List (GET/POST)
- Brands List (GET)
- Search Product (valid/missing parameter)
- Verify Login (valid/invalid/missing credentials)
- Create & Delete Account (with dynamic test data)

## 🚀 How to Run

### UI Tests
```bash
dotnet test
```

### API Tests
Import `PostmanTests/AutomationExercise API Tests.postman_collection.json`
into Postman and run with Collection Runner.
