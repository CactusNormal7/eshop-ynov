# Guide de Test - Discount Service (gRPC et HTTP)

## 📋 Table des matières
1. [Routes gRPC](#routes-grpc)
2. [Tester via Basket API (création de panier)](#tester-via-basket-api)
3. [Tester l'API HTTP REST avec Postman](#tester-api-http-avec-postman)

---

## 🔌 Routes gRPC

### Service : `DiscountProtoService`
**URL de base** : `http://localhost:6062` (gRPC)

Le service gRPC expose 4 méthodes pour gérer les coupons :

### 1. `GetDiscount` - Récupérer une réduction
- **Type** : Unary RPC
- **Request** : `GetDiscountRequest` avec `productName` (string)
- **Response** : `CouponModel` avec :
  - `id` (int32)
  - `productName` (string)
  - `description` (string)
  - `amount` (double) - Réduction fixe en €
  - `percentage` (double) - Réduction en pourcentage

**Exemple de requête** :
```json
{
  "productName": "IPhone X"
}
```

**Exemple de réponse** :
```json
{
  "id": 1,
  "productName": "IPhone X",
  "description": "IPhone X New",
  "amount": 0.0,
  "percentage": 30.0
}
```

### 2. `CreateDiscount` - Créer une réduction
- **Type** : Unary RPC
- **Request** : `CreateDiscountRequest` avec un `CouponModel`
- **Response** : `CouponModel` créé

**Exemple de requête** :
```json
{
  "coupon": {
    "productName": "Samsung Galaxy",
    "description": "Réduction Samsung",
    "amount": 0.0,
    "percentage": 25.0
  }
}
```

### 3. `UpdateDiscount` - Mettre à jour une réduction
- **Type** : Unary RPC
- **Request** : `UpdateDiscountRequest` avec un `CouponModel` modifié
- **Response** : `CouponModel` mis à jour

### 4. `DeleteDiscount` - Supprimer une réduction
- **Type** : Unary RPC
- **Request** : `DeleteDiscountRequest` avec un `CouponModel`
- **Response** : `DeleteDiscountResponse` avec `success` (bool)

---

## 🛒 Tester via Basket API (création de panier)

Le **Basket API** utilise automatiquement le **Discount Service** via gRPC lors de la création d'un panier.

### Étape 1 : Créer un coupon dans Discount Service

D'abord, assurez-vous qu'il existe un coupon pour un produit. Par défaut, il y a :
- **IPhone X** : 30% de réduction
- **Samsung 10** : 50% de réduction

### Étape 2 : Créer un panier avec des produits

**Endpoint** : `POST http://localhost:5051/baskets/john.doe`

**Corps de la requête** :
```json
{
  "cart": {
    "userName": "john.doe",
    "items": [
      {
        "productId": "550e8400-e29b-41d4-a716-446655440000",
        "productName": "IPhone X",
        "price": 1000.00,
        "quantity": 1,
        "color": "Black"
      },
      {
        "productId": "550e8400-e29b-41d4-a716-446655440001",
        "productName": "Samsung 10",
        "price": 800.00,
        "quantity": 1,
        "color": "White"
      }
    ]
  }
}
```

**Ce qui se passe** :
1. Le Basket API reçoit la requête
2. Pour chaque produit, il appelle le Discount Service via gRPC : `GetDiscount(productName)`
3. Il applique la réduction au prix : `nouveauPrix = prix - (prix * percentage / 100)`
4. Le panier est sauvegardé avec les prix réduits

**Exemple de résultat** :
- IPhone X : 1000€ → **700€** (30% de réduction)
- Samsung 10 : 800€ → **400€** (50% de réduction)
- **Total** : 1100€ (au lieu de 1800€)

### Étape 3 : Vérifier le panier créé

**Endpoint** : `GET http://localhost:5051/baskets/john.doe`

Vous verrez les prix avec les réductions déjà appliquées.

---

## 🌐 Tester l'API HTTP REST avec Postman

L'**Discount API** expose des endpoints REST pour gérer les réductions, codes promo et coupons avec des opérations CRUD complètes.

### Configuration Postman

**Base URL** : `http://localhost:5053`

**Headers** :
```
Content-Type: application/json
Accept: application/json
```

---

### 1. POST `/discounts/apply` - Appliquer des réductions

Appliquer un code promo ou des réductions automatiques à un panier.

**URL** : `POST http://localhost:5053/discounts/apply`

**Body** (JSON) :
```json
{
  "code": "PROMO10",
  "cartTotal": 150.00,
  "items": [
    {
      "productName": "IPhone X",
      "productId": "550e8400-e29b-41d4-a716-446655440000",
      "categories": ["Electronics", "Smartphones"],
      "price": 1000.00,
      "quantity": 1
    },
    {
      "productName": "Samsung 10",
      "productId": "550e8400-e29b-41d4-a716-446655440001",
      "categories": ["Electronics", "Smartphones"],
      "price": 800.00,
      "quantity": 1
    }
  ]
}
```

**Exemple sans code promo** (réductions automatiques uniquement) :
```json
{
  "cartTotal": 150.00,
  "items": [
    {
      "productName": "IPhone X",
      "price": 1000.00,
      "quantity": 1,
      "categories": ["Electronics"]
    }
  ]
}
```

**Response 200 OK** :
```json
{
  "originalTotal": 150.00,
  "discountAmount": 30.00,
  "finalTotal": 120.00,
  "appliedCode": "PROMO10",
  "appliedDiscounts": [
    {
      "type": "Coupon",
      "description": "IPhone X New - IPhone X",
      "amount": 20.00,
      "percentage": 30.0
    },
    {
      "type": "Code",
      "description": "Promotion 10%",
      "amount": 10.00,
      "percentage": 10.0
    }
  ]
}
```

---

### 2. GET `/discounts/validate/{code}` - Valider un code promo

Vérifier si un code promo est valide et obtenir ses informations.

**URL** : `GET http://localhost:5053/discounts/validate/PROMO10?cartTotal=100`

**Response 200 OK** (code valide) :
```json
{
  "isValid": true,
  "errorMessage": null,
  "codeInfo": {
    "codeValue": "PROMO10",
    "description": "Promotion 10%",
    "percentage": 10.0,
    "amount": 0.0,
    "minimumPurchaseAmount": 50.0,
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-12-31T23:59:59Z",
    "status": "Active",
    "isStackable": true,
    "maxCumulativeDiscountPercentage": 30.0
  }
}
```

**Response 200 OK** (code invalide) :
```json
{
  "isValid": false,
  "errorMessage": "Le code PROMO10 a expiré le 31/12/2023",
  "codeInfo": null
}
```

**Response 404 Not Found** (code inexistant) :
```json
{
  "isValid": false,
  "errorMessage": "Code promo non trouvé",
  "codeInfo": null
}
```

---

### 3. GET `/discounts/product/{productId}` - Réductions pour un produit

Récupérer toutes les réductions applicables à un produit spécifique.

**URL** : `GET http://localhost:5053/discounts/product/550e8400-e29b-41d4-a716-446655440000`

**Response 200 OK** :
```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "productName": "IPhone X",
  "coupon": {
    "id": 1,
    "description": "IPhone X New",
    "percentage": 30.0,
    "amount": 0.0,
    "startDate": null,
    "endDate": null,
    "status": "Active"
  },
  "automaticDiscounts": [
    {
      "type": "BlackFriday",
      "description": "Black Friday Sale",
      "percentage": 15.0,
      "amount": 0.0,
      "startDate": "2024-11-25T00:00:00Z",
      "endDate": "2024-11-30T23:59:59Z"
    }
  ]
}
```

---

## 📝 Routes CRUD - Codes Promo

### POST `/discounts/codes` - Créer un code promo

**URL** : `POST http://localhost:5053/discounts/codes`

**Body** :
```json
{
  "codeValue": "PROMO10",
  "description": "Promotion 10%",
  "percentage": 10.0,
  "amount": 0.0,
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "minimumPurchaseAmount": 50.0,
  "applicableCategories": ["Electronics", "Computers"],
  "isStackable": true,
  "maxCumulativeDiscountPercentage": 30.0,
  "isAutomatic": false,
  "automaticType": null,
  "tierRules": null
}
```

**Response 201 Created** :
```json
{
  "id": 1,
  "codeValue": "PROMO10",
  "description": "Promotion 10%",
  "percentage": 10.0,
  "amount": 0.0,
  "status": "Active",
  ...
}
```

### GET `/discounts/codes` - Lister les codes promo

**URL** : `GET http://localhost:5053/discounts/codes?status=Active&isAutomatic=false`

**Query Parameters** :
- `status` (optionnel) : Filtrer par statut (Active, Expired, Disabled, Upcoming)
- `isAutomatic` (optionnel) : Filtrer les codes automatiques (true/false)

**Response 200 OK** :
```json
[
  {
    "id": 1,
    "codeValue": "PROMO10",
    "description": "Promotion 10%",
    "percentage": 10.0,
    "status": "Active",
    ...
  }
]
```

### GET `/discounts/codes/{id}` - Récupérer un code

**URL** : `GET http://localhost:5053/discounts/codes/1`

### PUT `/discounts/codes/{id}` - Mettre à jour un code

**URL** : `PUT http://localhost:5053/discounts/codes/1`

**Body** : Même structure que CreateCodeRequest avec un `id` optionnel

### DELETE `/discounts/codes/{id}` - Supprimer un code

**URL** : `DELETE http://localhost:5053/discounts/codes/1`

**Response 204 No Content**

---

## 📝 Routes CRUD - Coupons

### POST `/discounts/coupons` - Créer un coupon

**URL** : `POST http://localhost:5053/discounts/coupons`

**Body** :
```json
{
  "productName": "MacBook Pro",
  "productId": "550e8400-e29b-41d4-a716-446655440002",
  "description": "Réduction MacBook",
  "percentage": 15.0,
  "amount": 0.0,
  "applicableCategories": ["Electronics", "Computers"],
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "minimumPurchaseAmount": 0.0
}
```

### GET `/discounts/coupons` - Lister les coupons

**URL** : `GET http://localhost:5053/discounts/coupons?status=Active&productName=IPhone`

**Query Parameters** :
- `status` (optionnel) : Filtrer par statut
- `productName` (optionnel) : Filtrer par nom de produit (recherche partielle)

### GET `/discounts/coupons/{id}` - Récupérer un coupon

**URL** : `GET http://localhost:5053/discounts/coupons/1`

### PUT `/discounts/coupons/{id}` - Mettre à jour un coupon

**URL** : `PUT http://localhost:5053/discounts/coupons/1`

### DELETE `/discounts/coupons/{id}` - Supprimer un coupon

**URL** : `DELETE http://localhost:5053/discounts/coupons/1`

---

## 🧪 Exemples de tests complets

### Test 1 : Créer un code promo et l'appliquer

1. **Créer un code promo via API REST** :
   ```
   POST http://localhost:5053/discounts/codes
   Body: {
     "codeValue": "PROMO10",
     "description": "Promotion 10%",
     "percentage": 10.0,
     "minimumPurchaseAmount": 50.0
   }
   ```
2. **Valider le code** :
   ```
   GET http://localhost:5053/discounts/validate/PROMO10?cartTotal=100
   ```
3. **Appliquer le code à un panier** :
   ```
   POST http://localhost:5053/discounts/apply
   Body: { "code": "PROMO10", "cartTotal": 200, "items": [...] }
   ```

### Test 2 : Créer un panier via Basket API avec réductions automatiques

1. **Créer un panier** :
   ```
   POST http://localhost:5051/baskets/testuser
   Body: {
     "cart": {
       "userName": "testuser",
       "items": [
         {
           "productName": "IPhone X",
           "price": 1000,
           "quantity": 1,
           "productId": "550e8400-e29b-41d4-a716-446655440000",
           "color": "Black"
         }
       ]
     }
   }
   ```
2. Le panier sera créé avec la réduction de 30% appliquée automatiquement (700€ au lieu de 1000€)

### Test 3 : Tester les réductions automatiques

1. **Créer une réduction automatique via API** :
   ```
   POST http://localhost:5053/discounts/codes
   Body: {
     "codeValue": "BLACKFRIDAY",
     "description": "Black Friday Sale",
     "percentage": 20.0,
     "isAutomatic": true,
     "automaticType": "BlackFriday",
     "startDate": "2024-11-25T00:00:00Z",
     "endDate": "2024-11-30T23:59:59Z"
   }
   ```
2. **Appliquer sans code** :
   ```
   POST http://localhost:5053/discounts/apply
   Body: { "cartTotal": 150, "items": [...] }
   ```
   La réduction automatique sera appliquée si elle correspond aux critères (dates, catégories, etc.)
3. La réduction automatique sera appliquée si elle correspond aux critères

---

## 📝 Notes importantes

### Ports
- **Discount gRPC** : `http://localhost:6062`
- **Discount API REST** : `http://localhost:5053`
- **Basket API** : `http://localhost:5051`

### Base de données
La base de données SQLite `discountDatabase` est partagée entre Discount.Grpc et Discount.API. Les migrations sont appliquées automatiquement au démarrage.

### Données par défaut
- **IPhone X** : 30% de réduction
- **Samsung 10** : 50% de réduction

### Swagger UI
Pour une interface interactive, accédez à :
```
http://localhost:5053/swagger
```

---

## 🔧 Outils pour tester gRPC

Pour tester les routes gRPC, vous pouvez utiliser :
- **BloomRPC** (application desktop)
- **grpcurl** (ligne de commande)
- **Postman** (support gRPC depuis la v10)
- **Code C# avec client gRPC généré**

### Exemple avec grpcurl

```bash
# Installer grpcurl
# macOS: brew install grpcurl

# Lister les services
grpcurl -plaintext localhost:6062 list

# Appeler GetDiscount
grpcurl -plaintext -d '{"productName": "IPhone X"}' \
  localhost:6062 discount.DiscountProtoService/GetDiscount
```

---

## ✅ Checklist de test

- [ ] Démarrer Discount.Grpc (port 6062)
- [ ] Démarrer Discount.API (port 5053)
- [ ] Démarrer Basket.API (port 5051)
- [ ] Vérifier Swagger : http://localhost:5053/swagger
- [ ] Tester POST /discounts/apply
- [ ] Tester GET /discounts/validate/{code}
- [ ] Tester GET /discounts/product/{productId}
- [ ] Tester POST /discounts/codes (créer un code)
- [ ] Tester GET /discounts/codes (lister les codes)
- [ ] Tester PUT /discounts/codes/{id} (modifier un code)
- [ ] Tester DELETE /discounts/codes/{id} (supprimer un code)
- [ ] Tester POST /discounts/coupons (créer un coupon)
- [ ] Tester GET /discounts/coupons (lister les coupons)
- [ ] Créer un panier via Basket API avec produits ayant des réductions
- [ ] Vérifier que les réductions sont appliquées correctement

