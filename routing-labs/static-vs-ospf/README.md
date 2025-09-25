# Static vs OSPF Routing Lab

This lab demonstrates how to configure **static routes**, replace them with **OSPF dynamic routing**, and verify end-to-end connectivity between two LAN segments across three routers.  
It also contrasts **manual route configuration** with **automatic route discovery** and reconvergence.

---

## 🎯 Goals

- Build connectivity between PC1 and PC2 using **static routes**.  
- Replace static routes with **single-area OSPF**.  
- Verify neighbor adjacencies, routing tables, and pings.  
- Simulate a link failure and observe **OSPF reconvergence**.

---

## 🖥️ Topology & IP Plan

### Topology
```

PC1 --- R1 --- R2 --- R3 --- PC2

````

### IP Addressing Plan

| Device | Platform     | Interface | IP Address     | Subnet Mask       | Purpose                     |
|--------|--------------|-----------|----------------|-------------------|-----------------------------|
| **PC1** | Alpine Linux | eth0      | 192.168.10.10  | 255.255.255.0     | Host in R1 LAN              |
| **R1**  | IOSv         | Gi0/0     | 192.168.10.1   | 255.255.255.0     | Gateway for PC1 LAN         |
|        |              | Gi0/1     | 10.0.12.1      | 255.255.255.252   | P2P link to R2 (Gi1)        |
| **R2**  | CSR1000v     | Gi1       | 10.0.12.2      | 255.255.255.252   | P2P link to R1 (Gi0/1)      |
|        |              | Gi2       | 10.0.23.1      | 255.255.255.252   | P2P link to R3 (Gi0/0)      |
| **R3**  | IOSv         | Gi0/0     | 10.0.23.2      | 255.255.255.252   | P2P link to R2 (Gi2)        |
|        |              | Gi0/1     | 192.168.20.1   | 255.255.255.0     | Gateway for PC2 LAN         |
| **PC2** | Alpine Linux | eth0      | 192.168.20.10  | 255.255.255.0     | Host in R3 LAN              |

---

## Link Connections (Routing Lab)

| Link / Subnet       | Device A (Interface) | Device B (Interface) |
|---------------------|-----------------------|-----------------------|
| 192.168.10.0/24     | R1 (Gi0/0)           | PC1 (eth0)           |
| 10.0.12.0/30        | R1 (Gi0/1)           | R2 (Gi1)             |
| 10.0.23.0/30        | R2 (Gi2)             | R3 (Gi0/0)           |
| 192.168.20.0/24     | R3 (Gi0/1)           | PC2 (eth0)           |

---

## Router Interface Configurations (Baseline)

### R1
```cisco
conf t
interface g0/0
 ip address 192.168.10.1 255.255.255.0
 no shut
interface g0/1
 ip address 10.0.12.1 255.255.255.252
 no shut
end
````

### R2

```cisco
conf t
interface g1
 ip address 10.0.12.2 255.255.255.252
 no shut
interface g2
 ip address 10.0.23.1 255.255.255.252
 no shut
end
```

### R3

```cisco
conf t
interface g0/0
 ip address 10.0.23.2 255.255.255.252
 no shut
interface g0/1
 ip address 192.168.20.1 255.255.255.0
 no shut
end
```

---

## 🖥️ PC1 & PC2 Configuration (Persistent via `vi`)

Both PCs use Alpine Linux. Network settings were configured with `vi` and saved in `/etc/network/interfaces`.

### PC1

```sh
vi /etc/network/interfaces

auto eth0
iface eth0 inet static
    address 192.168.10.10
    netmask 255.255.255.0
    gateway 192.168.10.1
```

### PC2

```sh
vi /etc/network/interfaces

auto eth0
iface eth0 inet static
    address 192.168.20.10
    netmask 255.255.255.0
    gateway 192.168.20.1
```

### Apply Changes

```sh
rc-service networking restart
```

---

## 📡 Static Routing

### R1

```cisco
ip route 192.168.20.0 255.255.255.0 10.0.12.2
```

### R2

```cisco
ip route 192.168.10.0 255.255.255.0 10.0.12.1
ip route 192.168.20.0 255.255.255.0 10.0.23.2
```

### R3

```cisco
ip route 192.168.10.0 255.255.255.0 10.0.23.1
```

---

## 🌐 Connectivity Tests (Static)

From **PC1**:

```sh
ping -c 3 192.168.20.10
```

From **PC2**:

```sh
ping -c 3 192.168.10.10
traceroute 192.168.10.10
```

---

## 🛠️ Additional Static Routes

* **R1 Floating Static**

```cisco
ip route 192.168.20.0 255.255.255.0 10.0.12.2 200
```

* **R2 Default Route**

```cisco
ip route 0.0.0.0 0.0.0.0 10.0.12.1
```

---

## 🗑️ Static Routes Removed (Clean Slate)

Removed with:

```cisco
no ip route ...
```

Verification:

```cisco
show ip route
```

Only directly connected routes should remain.

Ping PC1 → PC2 should now **fail**.

---

## 🖧 OSPF Routing (Single Area)

### R1

```cisco
router ospf 1
 router-id 1.1.1.1
 network 192.168.10.0 0.0.0.255 area 0
 network 10.0.12.0 0.0.0.3 area 0
```

### R2

```cisco
router ospf 1
 router-id 2.2.2.2
 network 10.0.12.0 0.0.0.3 area 0
 network 10.0.23.0 0.0.0.3 area 0
```

### R3

```cisco
router ospf 1
 router-id 3.3.3.3
 network 10.0.23.0 0.0.0.3 area 0
 network 192.168.20.0 0.0.0.255 area 0
```

---

## 🔍 Verification (OSPF)

```cisco
show ip ospf neighbor
show ip route ospf
```

Ping **PC1 → PC2** again. Should succeed.

---

## 🔻 Failure Simulation & Recovery

* Shut down R2’s Gi1 and Gi2 → neighbors drop, routes vanish.
* Restore with `no shutdown` → OSPF reconverges, routes reappear.

Verification:

```cisco
show ip ospf neighbor
show ip route ospf
```

---

## ✅ Reflection

* **Static routing** is predictable but brittle — every path must be entered manually.
* **OSPF** is dynamic, learns routes automatically, and reconverges quickly after failures.
* **Floating static + default routes** are useful as backups or at the edge.
* This lab validated both approaches with step-by-step configs, verification, and failure/recovery testing.

---

## Screenshots

1. **topology.png** – GNS3 topology layout
2. **ip\_plan.png** – IP Addressing Plan table
3. **r1\_ip\_int\_config.png** – R1 interface configuration
4. **r2\_ip\_int\_config.png** – R2 interface configuration
5. **r3\_ip\_int\_config.png** – R3 interface configuration
6. **pc1\_ip\_config.png** – PC1 IP settings output
7. **persistent\_settings\_pc1.png** – PC1 persistent network config
8. **r1\_static.png** – Static route on R1
9. **r2\_static.png** – Static route on R2
10. **r3\_static.png** – Static route on R3
11. **pc1\_to\_pc2\_ping.png** – PC1 pinging PC2 (static)
12. **pc2\_to\_pc1\_ping.png** – PC2 pinging PC1 (static)
13. **pc2\_traceroute.png** – PC2 traceroute to PC1
14. **r1\_static\_floating.png** – Floating static route on R1
15. **r2\_static\_default.png** – Default route on R2
16. **R2\_hostname\_logging.png** – R2 hostname/logging verification
17. **static\_routes\_gone.png** – Static routes removed
18. **ping\_fail.png** – Ping fails after removing statics
19. **r1\_ospf\_config.png** – OSPF configuration on R1
20. **r2\_ospf\_neighbor.png** – OSPF neighbor table on R2
21. **ospf\_ip\_route\_other\_pcs.png** – OSPF-learned routes on PCs
22. **pc1\_ping\_dynamic.png** – PC1 ping PC2 (OSPF)
23. **shutdown.png** – R2 interfaces shut down
24. **down\_no\_neighbor.png** – Neighbor down log on R1
25. **routes\_gone\_after\_shutdown.png** – No routes after shutdown
26. **ospf\_recovery.png** – OSPF recovery after re-enable
