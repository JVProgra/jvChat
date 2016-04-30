using System;
using System.IO;

public class Logger
{
	//=== Public Properties / Fields ===
	
	//The path to the log file (read only) 
	public string Path { get; private set; }
	
	//=== Private class variables === 
	
	//The stream writer for the log file 
	private StreamWriter writer; 
	
	//A locker object to lock between writers 
	private object logLocker; 
	
	//Constructor 
	public Logger(string path)
	{
		//Pass the parameters and setup the locker object 
		this.Path = path; 
		this.logLocker = new object();
		
		//Initialize the stream writer for the log file (we will need to handle an exception when initializing logger constructor)
		//in case the file is in use 
		this.writer = new StreamWriter(path, true); 
	}
	
	//Destructor 
	~Logger()
	{
		//When the logger object goes out of scope, close the stream 
		this.writer.Dispose();
	}
	
	
	//Call this function to write a new log entry 
	public void log(string log)
	{
		//Lock the object to prevent issues 
		lock(logLocker)
		{
			//Write a time stamp 
			this.writer.WriteLine("Log : {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()); 
			
			//Write the log entry provided 
			this.writer.WriteLine(log); 	
			
			//Flush the log data so it is saved to file (this will ensure data is saved to file even if program crashes before stream is closed) 
			this.writer.Flush();
		}
	}
}
