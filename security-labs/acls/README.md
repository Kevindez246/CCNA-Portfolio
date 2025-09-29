# 🔒 ACL Lab Report — 5 Attempts to Get It Right

## 🎯 Objectives

* Practice configuring **Standard and Extended ACLs**.
* Understand how ACL **placement (near source vs near destination)** and **direction (in vs out)** affect traffic.
* Demonstrate common mistakes and troubleshooting steps.
* Arrive at the **correct solution**: an Extended ACL applied near the source (R1 G0/1) to block **PC1 → PC2 ICMP** while allowing all else.

---

## 🖼️ Topology 
`acl_topology`
```
PC1 (192.168.10.10) ---- R1 ---- R2 ---- R3 ---- PC2 (192.168.20.10)
```

* PC1: 192.168.10.10/24 (gateway R1 G0/0)
* PC2: 192.168.20.10/24 (gateway R3 G0/1)
* Transit networks between R1–R2–R3: 10.0.x.x/30

---

# 🛠 Attempts and Outcomes

---

## 🔧 Attempt 1 (A1) — Standard ACL on R1 Inbound

```cisco
R1(config)# access-list 1 deny host 192.168.10.10
R1(config)# access-list 1 permit any
R1(config)# interface g0/0
R1(config-if)# ip access-group 1 in
```

### Result:

* PC1 → PC2 ping ❌ (blocked)
* PC1 → anywhere else ❌ (also blocked — overblocking)

📸 Screenshots:

* `show_access_a1` (ACL applied)
* `pc1_ping_fail_a1` (ping blocked, but too broad)

**Lesson:** Standard ACLs filter only by **source IP**, so placing them near the source blocks *all* traffic from PC1.

---

## 🔧 Attempt 2 (A2) — Standard ACL on R3 Inbound (Wrong Direction)

```cisco
R3(config)# access-list 1 deny host 192.168.10.10
R3(config)# access-list 1 permit any
R3(config)# interface g0/1
R3(config-if)# ip access-group 1 in
```

### Result:

* PC1 → PC2 ping ✅ (succeeded — ACL had no effect)

📸 Screenshots:

* `r3_in_acl_a2` (ACL applied)
* `pc1_ping_success_a2` (ping succeeded, showing ACL didn’t catch traffic)

**Lesson:** PC1’s traffic doesn’t *enter* R3 on G0/1 inbound, it *exits* there. Wrong direction = ACL not triggered.

---

## 🔧 Attempt 3 (A3) — Standard ACL on R3 Outbound

```cisco
R3(config)# interface g0/1
R3(config-if)# no ip access-group 1 in
R3(config-if)# ip access-group 1 out
```

### Result:

* PC1 → PC2 ping ❌ (blocked, correct)
* PC2 → PC1 ping ❌ (failed — replies dropped too)

📸 Screenshots:

* `pc2_ping_fail_a3` (PC2 ping fails)
* `pc1_ping_fail_a3` (PC1 ping fails)

**Lesson:** Standard ACLs are **stateless** and filter by source only. The echo replies from PC1 (source = 192.168.10.10) also matched the deny, breaking PC2 → PC1.

---

## 🔧 Attempt 4 (A4) — Extended ACL on R3 G0/1 Outbound

```cisco
R3(config)# ip access-list extended BLOCK-PC1-PC2
R3(config-ext-nacl)# deny ip host 192.168.10.10 host 192.168.20.10
R3(config-ext-nacl)# permit ip any any
R3(config)# interface g0/1
R3(config-if)# ip access-group BLOCK-PC1-PC2 out
```

### Result:

* PC1 → PC2 ping ❌ (blocked, correct)
* PC2 → PC1 ping ❌ (failed again — replies still blocked)

📸 Screenshot:

* `pc2_ping_fail_a4`

**Lesson:** Even with Extended ACLs, using a broad `deny ip` rule blocks replies as well, since they have the same source/destination pair (PC1 ↔ PC2).

---

## 🔧 Attempt 5 (A5) — Extended ACL on R1 G0/1 Outbound (Final Correct Solution)

```cisco
R1(config)# ip access-list extended BLOCK-PC1-PC2-ICMP
R1(config-ext-nacl)# deny icmp host 192.168.10.10 host 192.168.20.10 echo
R1(config-ext-nacl)# permit ip any any
R1(config)# interface g0/1
R1(config-if)# ip access-group BLOCK-PC1-PC2-ICMP out
```

### Result:

* PC1 → PC2 ping ❌ (blocked)
* PC2 → PC1 ping ✅ (works — replies not blocked)
* PC1 → other destinations ✅ (allowed)

📸 Screenshots:

* `r1_icmp_acl_a5` (ACL applied)
* `pc2_ping_success_a5` (PC2 ping succeeds, final validation)

**Lesson:**

* Extended ACLs allow precision filtering (here, only ICMP echo requests).
* Placing them **close to the source** ensures only the unwanted traffic is dropped, while replies and other flows remain intact.
* Using `echo` instead of `ip` prevents return traffic from being blocked.

---

# 🔍 Verification Commands

* `show access-lists` → verify deny counters increase only for PC1’s pings to PC2.
* `show ip interface g0/1` → confirm ACL applied outbound.
* Ping tests from PC1 and PC2 validate traffic flow.

---

# ✅ Final Takeaways

1. **A1:** Standard ACL near source = overblocking.
2. **A2:** Standard ACL wrong direction = no effect.
3. **A3:** Standard ACL outbound = blocked both directions (stateless issue).
4. **A4:** Extended ACL outbound on R3 = too broad, replies blocked.
5. **A5:** Extended ACL outbound on R1 G0/1, protocol-specific = correct solution.

**Key Exam/Real-World Insight:**

* Standard ACLs → simple source filters, place near destination.
* Extended ACLs → precise (source, destination, protocol), place near source.
* Always test both directions because ACLs are stateless.
* Narrow your deny statements (`icmp echo` vs `ip`) to avoid unintended blocks.
