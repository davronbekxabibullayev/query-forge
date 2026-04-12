# query-forge

# QueryForge ⚡

A lightweight dynamic LINQ query builder for 
.NET 8 / EF Core that supports filtering, sorting, and pagination using strongly typed rules.

---

# Architecture

FilterRule → ExpressionBuilder → Expression Tree → EF Core IQueryable

---

## 🚀 Features

- Dynamic filtering (FilterRule)
- Nested property support (e.g. `Region.Name`)
- String operations (Contains, StartsWith, EndsWith)
- Numeric and date comparisons
- IN / BETWEEN operators
- Dynamic sorting (OrderBy / ThenBy)
- Pagination support (Skip / Take)
- Fully EF Core compatible
- Expression tree based (no SQL injection risk)

---

## 📦 Installation

```bash
dotnet add package QueryForge

---

# Filtring

var result = dbContext.Cities
    .ApplyFilters(request.Filters)
    .ToList();

---

# Sorting

var result = dbContext.Cities
    .ApplySorting(request.Sorts)
    .ToList();

---

# Pagination

var result = dbContext.Cities
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .ToList();

---

# 📄 Request Model Example

{
  "page": 1,
  "pageSize": 10,
  "filters": [
    {
      "field": "Name",
      "operator": 6,
      "value": "tash"
    }
  ],
  "sorts": [
    {
      "field": "Region.Name",
      "desc": true
    }
  ]
}

---

# Supported Operators

| Operator       | Description          | Example Usage                  |
| -------------- | -------------------- | ------------------------------ |
| **Eq**         | Equals               | `Name == "Tashkent"`           |
| **Neq**        | Not equals           | `Status != "Active"`           |
| **Gt**         | Greater than         | `Age > 18`                     |
| **Gte**        | Greater or equal     | `Score >= 90`                  |
| **Lt**         | Less than            | `Price < 100`                  |
| **Lte**        | Less or equal        | `Rating <= 5`                  |
| **Contains**   | String contains      | `Name.Contains("ali")`         |
| **StartsWith** | Starts with          | `Name.StartsWith("Al")`        |
| **EndsWith**   | Ends with            | `Email.EndsWith("@gmail.com")` |
| **In**         | Value exists in list | `Id IN (1,2,3)`                |
| **Between**    | Range filter         | `Age BETWEEN 18 AND 30`        |
| **IsNull**     | Value is null        | `DeletedAt IS NULL`            |
| **IsNotNull**  | Value is not null    | `CreatedAt IS NOT NULL`        |