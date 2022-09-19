You must write a short app that must receives a packet every minute, parses the main header and internal structure headers and then outputs data from the internal structures when appropriate.

The packet received will be 20 bytes long consisting of a version, packet id, epoch time and two 7 byte structures called TSPVs. The two TSPV structures are each 7 bytes long where the first

two bytes are status bytes, the third byte is the [seconds] offset from the base epoch time and the last four bytes are a 32 bit float.



for example

01 04  62 55 76 5E  02 20 02 7F FF FF FF  03 10 0A 7F FF FF FF



The 2nd byte in the packet, indicates the PacketId.

PacketId of zero is reserved for, asking the device the latest data, when you start your app.

When the packet id on the device, reaches 255, it starts again from the beginning ie number 1 as zero is reserved.



From byte 3 to 6, is the date the packet has been generated on the device in epoch time (ie 32bit number which has the number of seconds since 1970 jan 1).

You may assume the second TSPV time offset is larger than the first TSPV time offset.



The source device which sends the packets sends TSPVs in consequent order and stores the last 4 sent packets, and occasionally if a packetid is received out of order, it means the data was missed and the

missing packets must be explicitly rerequested before the already received data inside can be processed. you can use the function       uint8_t* requestOldPacket(uint8 packetid);

The last 16 bytes - 7 to 20 - is a repeating pattern of 7 bytes TSPVs as mentioned above.

You cannot process TSPVs, with an earlier date than one already processed.



The output of the software will be to post the individual tspv float, as well as its time and status bytes. Use the function postTSPV(uint8_t stat1, uint8_t stat2, float pv);

The software has to gather and process as much of this data, without skipping, unless not possible.



Your test:



You need to write a routine(s) (and unit tests) process incoming data, retain packets in memory if out of order, ask the source device for missing packets and process the tspv into the output function.

Post a solution with the library, and unit tests, so we can run and evaluate it.



void receiveMSG(uint8_t * data, uint8_t length)

{

                //start your code



}



void unitTEST1()

{

                //call receiveMSG with test data

 

 

 

                //check contents of processed data using getLastData(uint32_t* time, float* pv);



}