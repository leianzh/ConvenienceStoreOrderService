# Business Rules｜商業規則

## 1. 核心原則

本專案將訂單流程拆成四條狀態線，避免所有狀態混在同一個欄位：

| 狀態線         | 用途                       |
| -------------- | -------------------------- |
| OrderStatus    | 訂單目前走到哪個階段       |
| ShipmentStatus | 物流目前走到哪個階段       |
| PaymentStatus  | 付款是否完成或取消         |
| RefundStatus   | 是否需要退款、退款是否完成 |

重要原則：

- 金流 API 成功後，才可以把狀態改成成功。
- 金流 API 失敗時，只記錄失敗原因，不把狀態改成成功。
- 請款失敗時不可出貨，也不可扣庫存。
- 退款 API 失敗時，退貨狀態維持完成，但退款狀態維持待處理。
- 庫存異動只在明確事件發生時執行，避免重複扣庫存或重複回補。

---

## 2. 訂單狀態流程

### 正常流程

```text
Processing
→ ReadyToShip
→ Shipped
→ Arrived
→ PickedUp
```

| 狀態        | 說明                             |
| ----------- | -------------------------------- |
| Processing  | 訂單建立完成，等待物流資料或付款 |
| ReadyToShip | 已取得寄件代碼，等待出貨         |
| Shipped     | 已出貨                           |
| Arrived     | 已到店                           |
| PickedUp    | 已取貨                           |

### 寄件前取消

只有以下狀態可以取消：

```text
Processing / ReadyToShip → Cancelled
```

取消成功時：

- 訂單改為 `Cancelled`
- 釋放預留庫存：`StockReserved -= qty`
- 若信用卡已授權但尚未請款，必須先成功取消授權
- 若取消授權失敗，訂單不可改為 `Cancelled`

### 出貨後退貨

只有以下狀態可以退貨：

```text
Shipped / Arrived / PickedUp → Returned
```

退貨成功時：

- 訂單改為 `Returned`
- 物流改為 `Returned`
- 回補實際庫存：`StockOnHand += qty`
- 清除 `ShippingCode寄件代碼`
- 保留 `TrackingNo` 作為歷史物流紀錄

---

## 3. 訂單與物流狀態對照表

| 事件             | OrderStatus | ShipmentStatus | 說明                         |
| ---------------- | ----------- | -------------- | ---------------------------- |
| 下單成功         | Processing  | Pending        | 訂單建立，等待物流資料或付款 |
| 物流資料填寫完成 | Processing  | Pending        | 已完成寄件與收件資料         |
| 取得寄件代碼     | ReadyToShip | ReadyToShip    | 賣家取得寄件代碼，等待出貨   |
| 出貨成功         | Shipped     | Shipped        | 商品已寄出                   |
| 到店             | Arrived     | Arrived        | 商品已送達門市               |
| 取貨成功         | PickedUp    | PickedUp       | 買家已取貨                   |
| 寄件前取消       | Cancelled   | Cancelled      | 尚未出貨，取消訂單           |
| 出貨後退貨       | Returned    | Returned       | 商品已退回                   |

---

## 4. 庫存規則

| 時機       | StockOnHand 實際庫存 | StockReserved 預留庫存 | 說明         |
| ---------- | -------------------: | ---------------------: | ------------ |
| 下單成功   |                 不變 |               `+= qty` | 預留庫存     |
| 出貨成功   |             `-= qty` |               `-= qty` | 正式扣庫存   |
| 寄件前取消 |                 不變 |               `-= qty` | 釋放預留庫存 |
| 退貨完成   |             `+= qty` |                   不變 | 回補實際庫存 |

庫存原則：

- 下單只預留，不正式扣庫存。
- 出貨才正式扣庫存。
- 寄件前取消只釋放預留庫存，不回補實際庫存。
- 出貨後退貨只回補實際庫存，不再異動預留庫存。
- 退款失敗不代表庫存要再回補一次。

---

## 5. 付款與退款規則

### COD

| 情境         | PaymentStatus | RefundStatus | 說明                 |
| ------------ | ------------- | ------------ | -------------------- |
| 下單成功     | Pending       | None         | 尚未收款             |
| 取貨成功     | Paid          | None         | COD 取貨付款完成     |
| 未取貨退回   | Cancelled     | None         | 未收款，不需退款     |
| 已付款後退貨 | Paid          | Requested    | 建立人工退款申請     |
| 人工退款完成 | Paid          | Refunded     | 賣家手動標記退款完成 |

### 信用卡

| 情境                 | PaymentStatus | IsCaptured是否請款 | RefundStatus | 說明                     |
| -------------------- | ------------- | -----------------: | ------------ | ------------------------ |
| 下單成功             | Pending       |              false | None         | 尚未付款                 |
| 信用卡交易成功       | Paid          |              false | None         | 授權成功，尚未請款       |
| 信用卡交易失敗       | Pending       |              false | None         | 維持待付款，可重新付款   |
| 出貨請款成功         | Paid          |               true | None         | 已請款，可以出貨         |
| 出貨請款失敗         | Paid          |              false | None         | 不可出貨，可重新請款     |
| 未請款前取消授權成功 | Cancelled     |              false | None         | 訂單可取消，釋放預留庫存 |
| 未請款前取消授權失敗 | Paid          |              false | None         | 訂單不可取消，不釋放庫存 |
| 已請款後退貨         | Paid          |               true | Requested    | 建立退款申請             |
| 退款成功             | Paid          |               true | Refunded     | 退款完成                 |
| 退款失敗             | Paid          |               true | Requested    | 退款仍待處理，可重試     |

---

## 6. 金流失敗時的商業處理

| 失敗情境         | 商業處理                                                                       |
| ---------------- | ------------------------------------------------------------------------------ |
| 信用卡交易失敗   | `PaymentStatus` 維持 `Pending`，允許重新付款；逾時後自動取消並釋放預留庫存     |
| 藍新請款失敗     | 訂單與物流維持 `ReadyToShip`，不可出貨，不可扣庫存，等待重試                   |
| 藍新退款失敗     | 訂單與物流維持 `Returned`，`RefundStatus` 維持 `Requested`，等待重試或人工查詢 |
| 藍新取消授權失敗 | 訂單不可取消，付款維持 `Paid`，預留庫存不可釋放                                |

---

## 7. 背景排程

| Job                                     | 用途                                                   |
| --------------------------------------- | ------------------------------------------------------ |
| `auto-cancel-expired-unpaid-orders`     | 信用卡付款逾時或付款未成功，自動取消訂單並釋放預留庫存 |
| `auto-cancel-expired-incomplete-orders` | 物流資料逾時未填，自動取消訂單並釋放預留庫存           |
| `clear-expired-shipping-codes`          | 寄件代碼逾期未寄件，自動清除，賣家可重新產生           |

---

## 8. 防重複操作規則

| 操作                | 防呆規則                                       |
| ------------------- | ---------------------------------------------- |
| 重複付款成功 Notify | 已付款的訂單不可再次改付款成功                 |
| 重複請款            | `IsCaptured = true` 時不可再次請款             |
| 重複退款            | `RefundStatus = Refunded` 時不可再次退款       |
| 重複取消授權        | `PaymentStatus = Cancelled` 時不可再次取消授權 |
| 重複取消訂單        | 已取消、已出貨、已退貨的訂單不可再次取消       |
| 重複退貨            | 已退貨的訂單不可再次退貨或重複回補庫存         |
