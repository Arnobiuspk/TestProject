using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;





namespace Server
{
    class Program
    {



        static int ports;
        static lectures[] alllectures;
        static List<Students> studentsonline;
        static int numberoflecture;



        static void Main(string[] args)
        {



            alllectures = new lectures[50];

            for(int i = 0; i < 50; i++)
            {

                alllectures[i] = new lectures();

            }


            numberoflecture = 0;
            studentsonline = new List<Students>();
            ports = 9000;


            Thread lecturess = new Thread(() => lecturethread());
            //lecturess.IsBackground = true;
            lecturess.Start();



            Thread stugreetings = new Thread(() => studentgreet());
            //lecturess.IsBackground = true;
            stugreetings.Start();



            Thread newlectureq = new Thread(() => newlecturerequest());
            //lecturess.IsBackground = true;
            newlectureq.Start();



            Thread outlectureq = new Thread(() => studentout());
            outlectureq.Start();



            Thread outteacher = new Thread(() => lectureout());
            outteacher.Start();


            Thread studentqq = new Thread(() => studentquit());
            studentqq.Start();



            Thread chattingg = new Thread(() => chatting());
            chattingg.Start();


            Thread fileshare = new Thread(() => Filesharing());
            fileshare.Start();

            Thread Givefile = new Thread(() => filedistributor());
            Givefile.Start();



            Console.WriteLine("everything running");






        }






        public static void lecturethread()
         {



            UdpClient newclass = new UdpClient(7000);
            





            while (true)
            {



                string listofstudents = "-i130339|i130040|i130039|i130059|i130023|i130023|i130032|i130023|i130024|i130232";


                lectures temp = new lectures();
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any,0);
                Byte[] receiveBytes = newclass.Receive(ref RemoteIpEndPoint);
                string Data = System.Text.Encoding.ASCII.GetString(receiveBytes);
               
                string[] splits = Data.Split('|');
                temp.teachername = splits[0];
                temp.lecturename = splits[1];
                temp.address = RemoteIpEndPoint.Address;

                lock(alllectures){
                    //alllectures.Add(temp);
                    alllectures[numberoflecture] = temp;

                }


                Console.WriteLine(temp.teachername);
                Console.WriteLine(temp.lecturename);
                Console.WriteLine(temp.address);



                Thread th1 = new Thread(() => screenbridge(ports + 1,numberoflecture));// THREAD FOR VIDEO SHARING
                th1.IsBackground = true;
                th1.Start();


                Thread th2 = new Thread(() => audiobridge(ports + 2,numberoflecture));// THREAD FOR AUDIO SHARING
                th2.IsBackground = true;
                th2.Start();


                int int1 = ports + 1;
                int int2 = ports + 2;

                string senddata = int1.ToString()+" "+int2.ToString() + listofstudents;

                Console.WriteLine(senddata);

                byte[] toBytes = Encoding.ASCII.GetBytes(senddata);


                newclass.Send(toBytes, toBytes.Length, RemoteIpEndPoint);


                ports = ports + 2;



                string message = "";




                lock (alllectures)
                {
                    for (int j = 0; j < alllectures.Length; j++)
                    {


                        if (alllectures[j].teachername != "")
                        {

                            string noofstudentsinclass = alllectures[j].allstudents.Count.ToString();

                            message = message + alllectures[j].teachername;
                            message = message + "-";
                            message = message + alllectures[j].lecturename;
                            message = message + "-";
                            message = message + noofstudentsinclass;
                            message = message + "|";




                        }





                    }


                }




                lock (studentsonline)
                {

                    for (int i = 0; i < studentsonline.Count; i++)
                    {

                        IPEndPoint sendingLectures = new IPEndPoint(studentsonline[i].address, 7002);


                       
                        byte[] toBytes1 = Encoding.ASCII.GetBytes(message);

                        newclass.Send(toBytes1, toBytes1.Length, sendingLectures);







                    }
                }



                numberoflecture++;










            }









        }








        public static void screenbridge(int port ,int classroom)
        {


            

            UdpClient screenshots = new UdpClient(port);
            UdpClient UDP_packet1 = new UdpClient();
            

            IPEndPoint localscreen = new IPEndPoint(IPAddress.Any, 7777);
            UDP_packet1.Client.SetSocketOption(
           SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UDP_packet1.Client.Bind(localscreen);

            string condition = "keepworking";





            while (condition == "keepworking")
            {


                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
                byte[] receiveBytes = screenshots.Receive(ref RemoteIpEndPoint);




                if (receiveBytes.Length > 0 && receiveBytes.Length < 10)
                {

                    string Data = Encoding.ASCII.GetString(receiveBytes);

                    if (Data == "finish")
                    {
                        condition = "stopworking";
                        string message = "done";
                        byte[] toBytes1 = Encoding.ASCII.GetBytes(message);



                        lock (alllectures[classroom])
                        {

                            for (int i = 0; i < alllectures[classroom].allstudents.Count; i++)
                            {

                                IPEndPoint temp = new IPEndPoint(alllectures[classroom].allstudents[i].address, 7778);
                                UDP_packet1.Send(toBytes1, toBytes1.Length, temp);
                                Console.WriteLine("quit message send to all video");


                            }
                        }





                        //[] toBytes1 = Encoding.ASCII.GetBytes(message);
                        screenshots.Send(toBytes1, toBytes1.Length, RemoteIpEndPoint);
                        break;
                    }



                }

                


                lock (alllectures[classroom])
                {
                    for (int i = 0; i < alllectures[classroom].allstudents.Count; i++)
                    {

                        IPEndPoint temp = new IPEndPoint(alllectures[classroom].allstudents[i].address, 7778);
                        UDP_packet1.Send(receiveBytes, receiveBytes.Length, temp);


                    }

                }




            }



            UDP_packet1.Close();
            screenshots.Close();
            Console.WriteLine("break"+port);





            


        }


        




        public static void audiobridge(int port , int classroom)
        {




            UdpClient audioshots = new UdpClient(port);
            UdpClient UDP_packet1 = new UdpClient();


            IPEndPoint localscreen = new IPEndPoint(IPAddress.Any, 9999);
            UDP_packet1.Client.SetSocketOption(
           SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UDP_packet1.Client.Bind(localscreen);

            string condition = "keepworking";



            while (condition == "keepworking")
            {

               

               // byte[] receiveBytes = new byte[4410];
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
                byte[] receiveBytes = audioshots.Receive(ref RemoteIpEndPoint);


            







                if (receiveBytes.Length > 0 && receiveBytes.Length < 10)
                {

                    string Data = Encoding.ASCII.GetString(receiveBytes);

                    if (Data == "finish")
                    {
                        condition = "stopworking";
                        string message = "done";
                        byte[] toBytes1 = Encoding.ASCII.GetBytes(message);



                        lock (alllectures[classroom])
                        {

                            for (int i = 0; i < alllectures[classroom].allstudents.Count; i++)
                            {

                                IPEndPoint temp = new IPEndPoint(alllectures[classroom].allstudents[i].address, 9998);
                                UDP_packet1.Send(toBytes1,toBytes1.Length,temp);
                                Console.WriteLine("quit message send to all audio");


                            }
                        }





                        //[] toBytes1 = Encoding.ASCII.GetBytes(message);
                        audioshots.Send(toBytes1, toBytes1.Length, RemoteIpEndPoint);
                        break;
                    }



                }



                lock (alllectures[classroom])
                {

                    for (int i = 0; i < alllectures[classroom].allstudents.Count; i++)
                    {

                        IPEndPoint temp = new IPEndPoint(alllectures[classroom].allstudents[i].address, 9998);
                        UDP_packet1.Send(receiveBytes, receiveBytes.Length, temp);


                    }
                }

                

            }




            UDP_packet1.Close();
            audioshots.Close();
            Console.WriteLine("break"+port);








        }



        public static void studentgreet()
        {
            

            //Console.WriteLine("hello");
            UdpClient newclass = new UdpClient(7001); // student greetings

            while (true)
            {

                Students temp = new Students();
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = newclass.Receive(ref RemoteIpEndPoint);
                string Data = System.Text.Encoding.ASCII.GetString(receiveBytes);

                temp.name = Data;
                temp.address = RemoteIpEndPoint.Address;


                lock (studentsonline)
                {

                    studentsonline.Add(temp);
                }


                Console.WriteLine(temp.name);
                Console.WriteLine(temp.address);

                string message = "";


                lock (alllectures)
                {

                    for (int i = 0; i < alllectures.Length; i++)
                    {


                        if (alllectures[i].teachername != "")
                        {

                            string noofstudentsinclass = alllectures[i].allstudents.Count.ToString();

                            message = message + alllectures[i].teachername;
                            message = message + "-";
                            message = message + alllectures[i].lecturename;
                            message = message + "-";
                            message = message + noofstudentsinclass;
                            message = message + "|";

                        }




                    }
                }




               

                byte[] toBytes = Encoding.ASCII.GetBytes(message);

                IPEndPoint x = new IPEndPoint(RemoteIpEndPoint.Address, 7002);

                newclass.Send(toBytes, toBytes.Length,x);




                Console.WriteLine("lecture send to new student");








            }



        }



        public static void newlecturerequest()
        {





            UdpClient newclass = new UdpClient(7003); 


            while (true)
            {


                Students temp = new Students();
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = newclass.Receive(ref RemoteIpEndPoint);
                string Data = System.Text.Encoding.ASCII.GetString(receiveBytes);

                //string[] splits = Data.Split('-');
                Console.WriteLine(Data);
                Console.WriteLine(RemoteIpEndPoint.Address);

                lock (studentsonline)
                {
                    for (int i = 0; i < studentsonline.Count; i++)
                    {


                        if (RemoteIpEndPoint.Address.ToString() == studentsonline[i].address.ToString())
                        {


                            temp.name = studentsonline[i].name;
                            temp.address = studentsonline[i].address;

                            break;



                        }













                    }
                }



                Console.WriteLine("student");
                Console.WriteLine(temp.address);
                Console.WriteLine(temp.name);



                lock (alllectures)
                {
                    for (int i = 0; i < alllectures.Length; i++)
                    {


                        if (alllectures[i].teachername == Data)
                        {

                            Console.WriteLine(alllectures[i].teachername);
                            alllectures[i].allstudents.Add(temp);



                        }


                    }

                }





                lock (alllectures)
                {


                    for (int i = 0; i < alllectures.Length; i++)
                    {


                        if (alllectures[i].teachername == Data)
                        {


                            string message = "";
                            for (int j = 0; j < alllectures[i].allstudents.Count; j++)
                            {



                                message = message + alllectures[i].allstudents[j].name;
                                message = message + "|";




                            }

                            //Console.WriteLine("sending list of student");

                            byte[] toBytes = Encoding.ASCII.GetBytes(message);

                            IPEndPoint x = new IPEndPoint(alllectures[i].address, 7006);

                            newclass.Send(toBytes, toBytes.Length, x);

                            break;





                        }


                    }










                }




                string message1 = "";


                lock (alllectures)
                {
                    for (int j = 0; j < alllectures.Length; j++)
                    {


                        if (alllectures[j].teachername != "")
                        {

                            string noofstudentsinclass = alllectures[j].allstudents.Count.ToString();

                            message1 = message1 + alllectures[j].teachername;
                            message1 = message1 + "-";
                            message1 = message1 + alllectures[j].lecturename;
                            message1 = message1 + "-";
                            message1 = message1 + noofstudentsinclass;
                            message1 = message1 + "|";



                        }




                    }
                }





                lock (studentsonline)
                {
                    for (int i = 0; i < studentsonline.Count; i++)
                    {

                        IPEndPoint sendingLectures = new IPEndPoint(studentsonline[i].address, 7002);




                        byte[] toBytes1 = Encoding.ASCII.GetBytes(message1);

                        newclass.Send(toBytes1, toBytes1.Length, sendingLectures);

                        //Console.WriteLine("lectures send"+message);





                    }
                }













            }

















        }

            

          public static void studentout()
        {


            UdpClient newclass = new UdpClient(7004); 



            while (true)
            {



                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = newclass.Receive(ref RemoteIpEndPoint);
                string Data = System.Text.Encoding.ASCII.GetString(receiveBytes);

                //string[] splits = Data.Split('-');
                //Console.WriteLine(splits[0]);
                Console.WriteLine("out request");
                Console.WriteLine(RemoteIpEndPoint.Address);


                int condition = 0;

                

                lock (alllectures)
                {

                    for (int i = 0; i < alllectures.Length; i++)
                    {

                        for (int j = 0; j < alllectures[i].allstudents.Count; j++)
                        {


                            if (RemoteIpEndPoint.Address.ToString() == alllectures[i].allstudents[j].address.ToString())
                            {


                                alllectures[i].allstudents.RemoveAt(j);
                                condition = 1;
                                Console.WriteLine("removed");
                                Console.WriteLine(alllectures[i].teachername);

                                break;




                            }









                        }

                        if (condition == 1)
                        {

                            break;
                        }












                    }






                }




                lock (alllectures)
                {


                    for (int i = 0; i < alllectures.Length; i++)
                    {


                        if (alllectures[i].teachername == Data)
                        {


                            string message = "";
                            for (int j = 0; j < alllectures[i].allstudents.Count; j++)
                            {



                                message = message + alllectures[i].allstudents[j].name;
                                message = message + "|";




                            }

                            //Console.WriteLine("sending list of student");

                            byte[] toBytes = Encoding.ASCII.GetBytes(message);


                            IPEndPoint x = new IPEndPoint(alllectures[i].address, 7006);

                            newclass.Send(toBytes, toBytes.Length, x);

                            break;





                        }


                    }










                }





                string message1 = "";


                lock (alllectures)
                {
                    for (int j = 0; j < alllectures.Length; j++)
                    {


                        if (alllectures[j].teachername != "")
                        {

                            string noofstudentsinclass = alllectures[j].allstudents.Count.ToString();

                            message1 = message1 + alllectures[j].teachername;
                            message1 = message1 + "-";
                            message1 = message1 + alllectures[j].lecturename;
                            message1 = message1 + "-";
                            message1 = message1 + noofstudentsinclass;
                            message1 = message1 + "|";



                        }




                    }
                }





                lock (studentsonline)
                {
                    for (int i = 0; i < studentsonline.Count; i++)
                    {

                        IPEndPoint sendingLectures = new IPEndPoint(studentsonline[i].address, 7002);




                        byte[] toBytes1 = Encoding.ASCII.GetBytes(message1);

                        newclass.Send(toBytes1, toBytes1.Length, sendingLectures);

                        //Console.WriteLine("lectures send"+message);





                    }
                }








            }









            }
        
        


            public static void lectureout()
        {


            UdpClient newclass = new UdpClient(7005); 




            while (true)
            {


                try {

                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 7005);
                    Byte[] receiveBytes = newclass.Receive(ref RemoteIpEndPoint);
                    string Data = System.Text.Encoding.ASCII.GetString(receiveBytes);

                    //string[] splits = Data.Split('-');
                    //Console.WriteLine(splits[0]);
                    Console.WriteLine("out Lecture");
                    Console.WriteLine(RemoteIpEndPoint.Address);
                    Console.WriteLine(Data);




                    lock (alllectures)
                    {
                        for (int i = 0; i < alllectures.Length; i++)
                        {


                            if (Data == alllectures[i].teachername)
                            {
                                Console.WriteLine(i);
                                alllectures[i] = new lectures();
                                break;


                            }










                        }
                    }




                    string message = "";


                    lock (alllectures)
                    {
                        for (int j = 0; j < alllectures.Length; j++)
                        {


                            if (alllectures[j].teachername != "")
                            {

                                string noofstudentsinclass = alllectures[j].allstudents.Count.ToString();

                                message = message + alllectures[j].teachername;
                                message = message + "-";
                                message = message + alllectures[j].lecturename;
                                message = message + "-";
                                message = message + noofstudentsinclass;
                                message = message + "|";



                            }




                        }
                    }





                    lock (studentsonline)
                    {
                        for (int i = 0; i < studentsonline.Count; i++)
                        {

                            IPEndPoint sendingLectures = new IPEndPoint(studentsonline[i].address, 7002);


                            

                            byte[] toBytes1 = Encoding.ASCII.GetBytes(message);

                            newclass.Send(toBytes1, toBytes1.Length, sendingLectures);

                            //Console.WriteLine("lectures send"+message);





                        }
                    }





                }

                

                catch(Exception e)
                {

                    Console.WriteLine(e);

                }



                Console.WriteLine("done with quitting");
                





            }












       }


        public static void studentquit()
        {



            UdpClient newclass = new UdpClient(7007);



            //Console.WriteLine("online");
            while (true)
            {


                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 7007);
                Byte[] receiveBytes = newclass.Receive(ref RemoteIpEndPoint);
                string Data = System.Text.Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine("studentquit");
                Console.WriteLine(Data);

                lock (studentsonline)
                {
                    for (int i = 0; i < studentsonline.Count; i++)
                    {

                        if(studentsonline[i].name == Data)
                        {

                            studentsonline.RemoveAt(i);

                        }





                    }
                }












          }










        }



        
        public static void chatting()
        {



                
            TcpListener serverSocket = new TcpListener(IPAddress.Any,8756);
            TcpClient clientSocket = default(TcpClient);
         


            serverSocket.Start();
            //Console.WriteLine(" >> " + "Chatting Started");





            while (true)
            {

                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("new cient");
                messagerun(clientSocket);








            }
















        }





        public static void messagerun(TcpClient x)
        {



            Thread chatting = new Thread(() => threadchattingudp(x));
            chatting.Start();
            






        }





        public static void threadchatting(TcpClient x)
        {


            byte[] bytesFrom = new byte[500];
            string message;

            NetworkStream networkStream = x.GetStream();
            networkStream.Read(bytesFrom, 0,500);
            message = System.Text.Encoding.ASCII.GetString(bytesFrom);
            //Console.WriteLine(message);
            string[] splits = message.Split('|');
            //Console.WriteLine(splits[0]);
            //Console.WriteLine(splits[1]);

            int number = -1;
            //networkStream.Flush();
            networkStream.Close();
            x.Close();




            Console.WriteLine("message recieved");
            Console.WriteLine(splits[1]);



            lock (alllectures)
            {



                

                for (int i = 0; i < alllectures.Length; i++)
                {


                    if (alllectures[i].teachername == splits[0])
                    {

                        TcpClient tcpclnt = new TcpClient();
                        tcpclnt.Connect(alllectures[i].address, 5555);
                        // use the ipaddress as in the server program

                        //Console.WriteLine("Connected");
                        Stream stm = tcpclnt.GetStream();


                        byte[] forward = System.Text.Encoding.ASCII.GetBytes(splits[1]);

                        //Console.WriteLine("Transmitting.....");

                        stm.Write(forward, 0, forward.Length);


                        stm.Close();
                        tcpclnt.Close();






                        number = i;
                        break;
                    }




                }

            }
                


                if(number>= 0)
                {

                    lock (alllectures[number])
                    {

                    Console.WriteLine("sending to students");
    
                        for (int i = 0; i < alllectures[number].allstudents.Count; i++)
                        {

                            //Console.WriteLine("sending student");
                            TcpClient tcpclnt = new TcpClient();
                            tcpclnt.Connect(alllectures[number].allstudents[i].address, 7787);
                            // use the ipaddress as in the server program

                            //Console.WriteLine("Connected");
                            Stream stm = tcpclnt.GetStream();
                                
                            
                            byte[] forward = System.Text.Encoding.ASCII.GetBytes(splits[1]);

                            //Console.WriteLine("Transmitting.....");

                            stm.Write(forward, 0, forward.Length);


                            stm.Close();
                            tcpclnt.Close();



                        }

                    }


                }
















            















        }





        public static void threadchattingudp(TcpClient x)
        {


            byte[] bytesFrom = new byte[500];
            string message;

            NetworkStream networkStream = x.GetStream();
            networkStream.Read(bytesFrom, 0, 500);
            message = System.Text.Encoding.ASCII.GetString(bytesFrom);
            //Console.WriteLine(message);
            string[] splits = message.Split('|');
            //Console.WriteLine(splits[0]);
            //Console.WriteLine(splits[1]);

            int number = -1;
            //networkStream.Flush();
            networkStream.Close();
            x.Close();


            Console.WriteLine("message recieved");
            Console.WriteLine(splits[1]);


            lock (alllectures)
            {


                UdpClient temp = new UdpClient();


                for (int i = 0; i < alllectures.Length; i++)
                {


                    if (alllectures[i].teachername == splits[0])
                    {

                       
                        byte[] forward = System.Text.Encoding.ASCII.GetBytes(splits[1]);
                        IPEndPoint temp1 = new IPEndPoint(alllectures[i].address, 5555);
                        temp.Send(forward, forward.Length, temp1);


                        






                        number = i;
                        break;
                    }




                }

                temp.Close();



            }



            if (number >= 0)
            {

                lock (alllectures[number])
                {
                    UdpClient temp = new UdpClient();


                    for (int i = 0; i < alllectures[number].allstudents.Count; i++)
                    {

                        Console.WriteLine("sending to student");
                        byte[] forward = System.Text.Encoding.ASCII.GetBytes(splits[1]);
                        IPEndPoint temp1 = new IPEndPoint(alllectures[number].allstudents[i].address,7787);
                        temp.Send(forward, forward.Length, temp1);


                        



                    }


                    temp.Close();

                }


            }




        }




        public static void Filesharing()
        {




            TcpListener serverSocket = new TcpListener(IPAddress.Any, 7050);
            //TcpClient clientSocket = default(TcpClient);



            serverSocket.Start();
            //Console.WriteLine(" >> " + "Chatting Started");





            while (true)
            {

                Socket clientSocket = serverSocket.AcceptSocket();
                Console.WriteLine("new cient file sharing");
                getfile(clientSocket);


            }

        }



        public static void getfile(Socket x)
        {



            Thread files = new Thread(() => getfile1(x));
            files.Start();



        }



        public static void getfile1(Socket x)
        {

            string fileName = string.Empty;
            NetworkStream networkStream = new NetworkStream(x);
            int thisRead = 0;
            int blockSize = 1024;
            Byte[] dataByte = new Byte[blockSize];

            string foldername = "serverfiles";
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = path + @"\";
            Directory.CreateDirectory(path + foldername);
            //string folderPath = @"C:\Users\Farhan\Documents\serverfiles\";
            string folderPath = path + foldername + @"\";

            int receivedBytesLen = x.Receive(dataByte);
            int fileNameLen = BitConverter.ToInt32(dataByte, 0);
            fileName = Encoding.ASCII.GetString(dataByte, 4, fileNameLen);
            Stream fileStream = File.OpenWrite(folderPath + fileName);
            fileStream.Write(dataByte, 4 + fileNameLen, (1024 - (4 + fileNameLen)));
            while (true)
            {
                thisRead = networkStream.Read(dataByte, 0, blockSize);
                fileStream.Write(dataByte, 0, thisRead);
                if (thisRead == 0)
                    break;
            }
            fileStream.Close();


            Console.WriteLine("file downloaded");


            IPEndPoint remoteIpEndPoint = x.RemoteEndPoint as IPEndPoint;
            IPAddress temp = remoteIpEndPoint.Address;

            
            networkStream.Close();
            Console.WriteLine(temp);
            
            lock (alllectures)
            {
                for (int i = 0; i < alllectures.Length; i++)
                {
                    
                    if (temp.Equals(alllectures[i].address))
                    {
                        //Console.WriteLine("teacher found");
                        UdpClient temp2 = new UdpClient();


                        for (int j = 0; j < alllectures[i].allstudents.Count; j++)
                        {

                            //Console.WriteLine("sending to student");
                            string message = "file:" + fileName;
                            byte[] forward = System.Text.Encoding.ASCII.GetBytes(message);
                            IPEndPoint temp1 = new IPEndPoint(alllectures[i].allstudents[j].address, 7787);
                            temp2.Send(forward, forward.Length, temp1);

                        }


                        temp2.Close();
                        break;

                    }

                }
            }

            x.Close();


        }



        public static void filedistributor()
        {




            TcpListener serverSocket = new TcpListener(IPAddress.Any, 7051);
            //TcpClient clientSocket = default(TcpClient);



            serverSocket.Start();
            //Console.WriteLine(" >> " + "Chatting Started");





            while (true)
            {

                TcpClient clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("new student request to recieve file");
                givefile(clientSocket);


            }

        }



        public static void givefile(TcpClient x)
        {



            Thread files = new Thread(() => givefile1(x));
            files.Start();



        }



        public static void givefile1(TcpClient x)
        {



            byte[] bytesFrom = new byte[500];
            string filename;

            NetworkStream networkStream = x.GetStream();
            networkStream.Read(bytesFrom, 0, bytesFrom.Length);
            filename = System.Text.Encoding.UTF8.GetString(bytesFrom);
            //filename = filename.Trim();
            //char[] charsToTrim = { ' ', '\t' };
            //string result = filename.Trim(charsToTrim);
            //string illegal = "\"M\"\\a/ry/ h**ad:>> a\\/:*?\"| li*tt|le|| la\"mb.?";
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                filename = filename.Replace(c.ToString(), "");
            }


            //Console.WriteLine(message);
            Console.WriteLine(filename);


            string foldername = "serverfiles";
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = path + @"\";
            //Directory.CreateDirectory(path + foldername);
            //string folderPath = @"C:\Users\Farhan\Documents\serverfiles\";
            string filePath = path + foldername + @"\" + filename;



            Console.WriteLine(filePath);
            byte[] fileData = File.ReadAllBytes(filePath);
            networkStream.Write(fileData, 0, fileData.GetLength(0));
            networkStream.Close();
            x.Close();

            Console.WriteLine("file transfered to student");
            
















        }













    }
}
