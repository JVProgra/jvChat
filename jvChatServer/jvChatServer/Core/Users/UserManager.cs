using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace jvChatServer.Core.Users
{
    class UserManager
    {
        //=== Public properties ===

        /// <summary>
        /// The user database 
        /// </summary>
        public List<User> Users { get; private set; }

        //Constructor 
        public UserManager()
        {
            Users = new List<User>();     
        }

        /// <summary>
        /// This method is used to add new users to the user database 
        /// </summary>
        /// <param name="u">The new user account</param>
        /// <returns>True if the user is added or false if the account already exists / if an error occurs.</returns>
        public bool addUser(User u)
        {
            //use LINQ to locate an account with the same name 
            var user = getUserByName(u.Name);

            //If an account object is found matching then return false 
            if (user != null)
                return false;

            //Else add the account to the database 
            Users.Add(u);

            //Return true because everything worked 
            return true; 
        }

        /// <summary>
        /// This function will validates a users credentials 
        /// </summary>
        /// <param name="name">Username</param>
        /// <param name="password">Password</param>
        /// <returns>A user object if the information is correct or null if it's incorrect</returns>
        public User ValidateUser(string name, string password)
        {
            //use LINQ to find an account object with the same name 
            var user = getUserByName(name);

            //If no account is found 
            if (user == null)
                //return null 
                return null;
            //If an account is found check the password 
            else if (user.Password == password)
                //Return account if password matches 
                return user;
            //Else return null cause password is wrong 
            else
                return null; 
        }

        /// <summary>
        /// This function can be used to locate a user account by name 
        /// </summary>
        /// <param name="name">The name of the requested user.</param>
        /// <returns>A user instance of the located account or null if none were found.</returns>
        public User getUserByName(string name)
        {
            return Users.FirstOrDefault(n => n.Name == name); 
        }

        /// <summary>
        /// Call this method to update an accounts permission level 
        /// </summary>
        /// <param name="user">The user name of the account you want to update</param>
        /// <param name="level">The new permissions level for the account</param>
        public void setPermissions(string user, AccountLevel level) //chmod? :D 
        {
            //Check to see if an account exists
            var u = getUserByName(user);

            //If an account does not, return null 
            if (u == null)
                return;
            else
            {
                //Locate account and update it
                
                //Loop through each account 
                for (int i = 0; i < Users.Count; i++)
                    //Check each username for a matc 
                    if (Users[i].Name == user)
                        //update account information 
                        Users[i].Level = level; 
            }
        }

        /// <summary>
        /// Call this function to initialize a new database controller from a file 
        /// </summary>
        /// <param name="path">The path to the database file</param>
        /// <returns>The user controller or throws an exception</returns>
        public static UserManager FromFile(string path)
        {
            if (!File.Exists(path))
                throw new Exception("Unable to locate user controller database file.");

            //Create a variable to store the new controller in
            UserManager uCon = new UserManager();

            try
            {
                //Try to open the database file (Could add decryption in here somewhere :D ) 
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    //Createa binary reader to read the file 
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        //Read the amount of users which is stored at the beginning of the DB file 
                        int amountOfUsers = br.ReadInt32(); 

                        //Loop through each users in the file 
                        for(int i = 0; i < amountOfUsers; i++)
                        {
                            //Add the user to the controller 
                           uCon.addUser(new User { Name = br.ReadString(), Password = br.ReadString(), Level = (AccountLevel)br.ReadInt32() });
                        }
                    }
                }
            }
            //Exception; file probably in use or something 
            catch (Exception ex) 
            {
                //Log the error message here 
            }

            //Load database here 
            return uCon;
        }


        /// <summary>
        /// Call this function to save all users to the database 
        /// </summary>
        /// <param name="path">The path to the database file to save to</param>
        /// <returns>True if the database is saved correctly</returns>
        public bool saveDatabase(string path)
        {
            //If there are no users in the database there is nothing we can do so return false 
            if (Users.Count == 0)
                return false; 

            try
            {
                //Create a file stream to save the database to 
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    //Create a binary writer to save the database with (for formatting purposes)
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        //Start with the amount of data entrys for optimization when loading
                        bw.Write(Users.Count); 

                        //Loop through each user in the database 
                        foreach(User u in Users)
                        {
                            //Write the account name 
                            bw.Write(u.Name);

                            //Write the account password 
                            bw.Write(u.Password);

                            //Write the account permissions 
                            bw.Write((int)u.Level);
                        }
                    }
                }
                
                //If no error occured up until this point then everything should be fine so return true 
                return true; 
            }
            catch (Exception ex)
            {
                //probably some kind of file error we can return here 
            }

            //Return false because something failed 
            return false; 
        }


    }
}
