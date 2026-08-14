# High-level component / domain view

```mermaid
flowchart LR
  subgraph Deployable["Admiral.Booking API - modular monolith"]
    subgraph BookingMgmt["Module: Booking Management"]
      BC[Booking aggregate]
      BH[ConfirmBooking handler]
    end
    subgraph VoyageMgmt["Module: Voyage and Capacity"]
      VC[Voyage aggregate]
      CR[CapacityReservation entity]
    end
    DS[BookingConfirmation domain service]
    ID[IdempotencyRecords]
    OX[OutboxMessages]
    IX[InboxMessages]
    API[ASP.NET controllers]
  end

  SQL[(SQL Server)]
  Downstream[Downstream consumer / test double]

  API --> BH
  BH --> ID
  BH --> DS
  DS --> BC
  DS --> VC
  VC --> CR
  BH --> OX
  OX --> Downstream
  Downstream --> IX
  BH --> SQL
  VC --> SQL
  OX --> SQL
```

Deployment boundary: one process. Database boundary: one SQL Server database (command and query connections may be the same). Module boundary: folders and aggregates, not network calls.
