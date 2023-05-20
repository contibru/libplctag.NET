# libplctag - I need performance, give me it

Whoa boy, whoa! 

1. What do we mean by performance?
2. How do I know what my problem is?
3. Communications using libplctag.
4. How can libplctag help me achive my performance goals?

Although libplctag supports multiple protocols and PLC Types, this article will concentrate on the combination of Logix PLC. Some of this discussion will be applicable to other scenarios.

## What do we mean by performance? It depends...

Asking for better performance is a bit like asking for a better vehicle - it depends on your goals:

* For a race-car driver, quickly accelerating/decelerating might be the most important aspect - and the top speed is less important.
* For a parent with a large family of school-aged children, maybe a car with extra seats is better - you can drive slowly, but it avoids the need for multiple trips.
* For a truck driver, an engine that lasts longer between maintenance - better return on investment.

I'm not a parent, nor a professional driver, but I am confident that "better performance" depends on your goals.

It is important that you are as clear as possible on what your goal is, or the specific problem that you have, so that you can get the "performance" that you need.

With respect to industrial communication networks, here are some other words that you could use instead of "performance":

* Latency - the span of time between sending a request and recieving it.
* Throughput - the number of requests within a given duration.
* Bandwidth - a combination of latency and throughput.
* Reliability - what confidence can you have that the request will reach its intended destination?
* Determinism - are you expecting a request every 1 second? What jitter is acceptable?
* Responsiveness - the user/operator's _perception_ of the system responding to something that they do (e.g. press a button).
* Power Consumption - this can sometimes be the critical performance requirement  particularly in Internet-Of-Things applications.

There are also many other problems that loosely could come under the banner of "performance", some examples are PC CPU utilisation, PLC CPU utilisation, network contention, Memory Consumption, Garbage Collection pressure, Exception Handling overhead, etc.

## So, how do I know which problem I want to fix?

In some ways, performance problems are much more difficult to fix than functional problems.
There are many reasons for this, but it boils down to being able to reproduce the problem - you often can't.

Starting with the assumption that you have a problem - presumably a vague complaint of some kind.

1. Identify contributing factors.
2. Develop a test harness that exercises those factors independently.
3. Analyze the impact of each factor, and the interplay of them against each other.


Software is complex, and identifying contributing factors is hard.
After-all, if you knew what the problem is you wouldn't be reading this document!


## Data Access with libplctag

libplctag internally has an implementation of Ethernet/IP Explicit Messaging.

EtherNet/IP  is an industrial network protocol that adapts the [Common Industrial Protocol](https://en.wikipedia.org/wiki/Common_Industrial_Protocol) (CIP) to standard Ethernet.
In Ethernet/IP, "IP" stands for **Industrial** Protocol (from CIP), not **Internet** Protocol (which is what IP stands for in "IP Address", and "TCP/IP").
Confusingly, it does actually use the Internet Protocol for encapsulation.

Ethernet/IP sits in the "Process Layer" in the [OSI model](https://en.wikipedia.org/wiki/OSI_model), and utilises either User Datagram Protocol (UDP) or Transmission Control Protocol (TCP) for transport layer, Internet Protocol (IP) for Network layer, and Ethernet with CSMA/CD for Data Link layer. For the physical layer, any media can be used (Cat5, WiFi, Fibre, etc).

A design assumption made during the development of Ethernet/IP is that it must be able to share a Data Link with other TCP/IP-based applications. This is seen to be valuable because applications based on the TCP/IP stack have become ubiquitous and hold significant mindshare, creating a compelling return on investment proposition for organizations.
https://www.odva.org/wp-content/uploads/2020/06/PUB00123R1_Common-Industrial_Protocol_and_Family_of_CIP_Networks.pdf

However, there are downsides to this approach (notably protocol efficiency), and not all networking technologies have prioritised coexistence with other network users. [EtherCAT](https://en.wikipedia.org/wiki/EtherCAT) is one example that has taken a different approach.

Ethernet/IP itself can be broken down into categories:
* Explicit Messaging (TCP/IP) - this is a request/response model
* Implicit Messaging (UDP/IP) - 
    * Polled - Master sequentially queries all slave devices, who respond individually. 
    * Strobed - Master sends single multicast to all slave devices, who repond individually.
    * Cyclic - At a known interval, slaves send data to the master without being prompted. This is recommended by Rockwell as providing the best balance of Data Integrity and network traffic optimization 
    * Change of State - The slave decides when to send data to the master. Similar to Cyclic, except that data is produced in response to an event, rather than at a time interval. 

Note: libplctag only supports Explicit Messaging, and does not support Implicit Messaging.

[(pg10)](https://literature.rockwellautomation.com/idc/groups/literature/documents/wp/enet-wp001_-en-p.pdf).


### Whats in a libplctag Request?

Now that we've identified which other networking technologies are used, lets examine the detail of what goes into a libplctag request.

1. libplctag Request
2. CIP Frame
3. TCP Frame
4. IPv4 or IPv6 Packet: https://en.wikipedia.org/wiki/Internet_Protocol_version_4#Packet_structure
5. Ethernet Frame: https://www.omnisecu.com/tcpip/ethernet-frame-format.php
   Ethernet with CSMA/CD




## Concurrency Model

The library will automatically try to do two things for performance if you are working with a ControlLogix or a CompactLogix:

It will attempt to negotiate a large packet size with the PLC. Older CIP PLCs generally have roughly 500-byte packet sizes. Newer ones can support 4000-byte packets.
It will attempt to pack as many requests as it can into one packet to the PLC.

libplctag internally queues requests and, if <insert heuristic here>, it will batch those requests to the target devices.
This will <explain mechanism>, and in some cases will increase data throughput.

There are three concurrency models supported by libplctag that allows you to make use of this effect.
* Auto-Read/Auto-Write - see [link]
* Async - see [link]
* Using multiple threads - see [link]

The concurrency model you select does not affect libplctag's ability to batch requests.
  
  
  
## References
   
* [EtherNet/IP Network Devices](https://literature.rockwellautomation.com/idc/groups/literature/documents/um/enet-um006_-en-p.pdf)   
* [Logix 5000 Controllers Data Access](https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm020_-en-p.pdf)
* [Logix 5000 Controllers General Instructions](https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1756-rm003_-en-p.pdf)
* [Logix 5000 Controllers I/O and Tag Data](https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm004_-en-p.pdf)
* [Logix 5000 Controllers Design Considerations](https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1756-rm094_-en-p.pdf)
* [EtherNet/IP: Industrial Protocol White Paper](https://literature.rockwellautomation.com/idc/groups/literature/documents/wp/enet-wp001_-en-p.pdf)
* https://www.sciencedirect.com/science/article/pii/S1474667016351813
* https://stackoverflow.com/questions/17146332/how-can-i-study-ethercat-without-any-background  
* https://www.ethercat.org/en/downloads.html
* https://www.deltamotion.com/support/webhelp/rmctools/Communications/Ethernet/Supported_Protocols/EtherNetIP/EtherNet_IP_I_O_Performance.htm
* https://www.odva.org/wp-content/uploads/2020/05/PUB00213R0_EtherNetIP_Developers_Guide.pdf
* https://www.odva.org/wp-content/uploads/2021/05/PUB00138R7_Tech-Series-EtherNetIP.pdf   
* https://en.wikipedia.org/wiki/SERCOS_interface
* https://github.com/EIPStackGroup/OpENer
* https://github.com/nimbuscontrols/EIPScanner
* [EZ-EDS](https://www.odva.org/subscriptions-services/additional-tools/ez-eds-download/)
* Protocol Conformance Test Software Tool
