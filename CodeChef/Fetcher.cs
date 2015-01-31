/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2015, Aman Priyadarshi                                                    *
 * All rights reserved.                                                                    *
 *                                                                                         *
 * Redistribution and use in  source and  binary  forms,  with  or  without  modification  *
 * are permitted provided that the following conditions are met:                           *
 *                                                                                         *
 *        1. Redistributions of  source  code  must  retain the  above  copyright  notice  *
 *           this list of conditions and the following disclaimer.                         *
 *        2. Redistributions in  binary form  must  reproduce  the above copyright notice  *
 *           this list of conditions and the following  disclaimer  in  the documentation  *
 *           and/or other materials provided with the distribution.                        *
 *                                                                                         *
 * THIS SOFTWARE IS PROVIDED BY THE  COPYRIGHT HOLDERS  AND  CONTRIBUTORS "AS IS" AND ANY  *
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED  TO,  THE IMPLIED WARRANTIES  *
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE  DISCLAIMED. IN  NO  EVENT  *
 * SHALL THE   COPYRIGHT   HOLDER  OR  CONTRIBUTORS  BE  LIABLE  FOR ANY DIRECT, INDIRECT  *
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR  CONSEQUENTIAL DAMAGES  (INCLUDING, BUT NOT LIMITED  *
 * TO, PROCUREMENT OF SUBSTITUTE GOODS  OR  SERVICES; LOSS OF USE, DATA,  OR  PROFITS; OR  *
 * BUSINESS INTERRUPTION) HOWEVER CAUSED  AND  ON  ANY  THEORY OF  LIABILITY,  WHETHER IN  *
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY  *
 * WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/*
 * Technical Description:
 * .NET version: 2.0 or later
 * Language: C#
 * Library: mscorlib
 */

using System;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace Codechef
{
    public class Fetcher
    {
        /// <summary>
        /// Programs Startpoint;
        /// Above license is really necessary; but also it makes files beautiful; I really wasted 5 minutes to align
        /// license in such style xD i'm cool dude na? xD
        /// </summary>
        public static void Main()
        {
            /* Setup of fancy thing although i'm not fancy xD */
            Console.Title = "Codechef Submission Fetcher by Aman Priyadarshi";
            Console.ForegroundColor = ConsoleColor.Green;

            /* Well I really like to please the user */
            Console.Write("Please enter your user id:");
            string UID = Console.ReadLine();

            /* I can do this without using "using" keyword; But i seriously don't know why i did this :/ */
            using (var client = new WebClient())
            {
                string Profile = client.DownloadString(string.Format("http://www.codechef.com/users/{0}", UID));
                //Well codechef uses rewrite url and shows us like this "QUESTION,PROFILE_ID"
                string QuestionEntryPattern = string.Format(",{0}", UID);
                int index = 0, start;
                var QuestionsList = new List<string>();

                /* Well first find number of solutions 
                 * i'll tell you why i did this, Actually i'm not pretty sure that my program will even able to
                 * find number of solutions; so i first try to do that :P
                 */
                do
                {
                    //Well get IndexOf same question patter after current one.
                    index = Profile.IndexOf(QuestionEntryPattern, index + 1);
                    if (index != -1)
                    {
                        for (start = index; start >= 0; start--)
                        {
                            //Well this is a kinda hack, But a good one; so who cares XD
                            if (Profile[start] == '"')
                                break;
                        }
                        start++;
                        //Add the question url to list
                        QuestionsList.Add(Profile.Substring(start, index - start));
                    }
                }
                while (index != -1);

                //Hehe, This was the first happiness while writing this, because it outputs the correct value.
                Console.WriteLine("Number of Questions detected::" + QuestionsList.Count);
                /* well just don't ask why i asked user to continue,
                 * well its simple, Those who don't want to continue will exit the program so intead 
                 * of running my program continously; why don't we ask to user
                 * Hehe Nice joke? xD
                 */
                Console.Write("Do you want to continue? (y/n)");
                string response = Console.ReadLine();

                /*You know what, When i write this; i was pretty sure that maybe user will type "Yes"
                 *I can make it that compatible also, but why should i; When i give only two options
                 *to user, y/n; Hence if user is not paying too much attention then i really don't care
                 *to show him output :P So, Terminate the program in else XD
                 */
                if (response == "y")
                {
                    //I really don't why i named this variable "SuccessCount" :(
                    int SuccessCount = 0;
                    const string TMP001 = "/viewsolution/";//Just a random name which striked into my mind why writing code
                    foreach (var question in QuestionsList)
                    {
                        Console.Write(question);
                        Console.Write("...");
                        string Submissions = client.DownloadString(string.Format("http://www.codechef.com/{0},{1}", question, UID));
                        /* Well here is a simple logic, If the submission have hidden solutions
                         * Then it means that page won't have any link which contains "viewsolution"
                         * I'm cool na :P i know xD Just kidding :D
                         */
                        index = Submissions.IndexOf(TMP001);
                        if (index == -1)
                        {
                            Console.WriteLine("failed");
                            continue;
                        }

                        //I'm wishing this "SuccessCount" increments parallely with my real success meter xD
                        SuccessCount++;

                        /*Below is kinda parsing logic; you will have a fun while understanding it
                         *So, i leave it on you :P
                         *lolz, Actually this is not the reason, reason is i'm lazy at commenting things
                         *so to save my comments i'm leaving on you xD
                         */
                        index += TMP001.Length;
                        for (start = index; ; start++)
                            if (Submissions[start] == '\'')
                                break;
                        string solID = Submissions.Substring(index, start - index);
                        for (index = start; ; index--)
                            if ((Submissions[index] == '<') && (Submissions[index + 1] == '/'))
                                break;
                        for (start = index; ; start--)
                            if (Submissions[start] == '>')
                                break;
                        start++;
                        string LANG = Submissions.Substring(start, index - start);
                        //Finally _/\_ save it to local pc :D
                        SaveCodeFile(client.DownloadString(string.Format("http://www.codechef.com/viewplaintext/{0}", solID)), question, LANG);
                        Console.WriteLine("done");
                    }

                    /* Final statistics; i know very few really cares of you
                     * But don't think i made really this for users xD
                     * Just to verify, how much submissions fetched while debugging code i made this line of code.
                     */
                    Console.WriteLine(string.Format("Fetching Done successfully:: {0} out of {1}", SuccessCount, QuestionsList.Count));

                    //This is a kinda necessary; user allowed my program to run, thats really big thing for me :'(
                    Console.WriteLine("Thank you for using Codechef solution fetcher");
                    Console.WriteLine();//Style xD

                    //Its nothing; Just a kinda show off =P you know, i guess XD
                    Console.WriteLine("Application is created by one and only ;)");
                    Console.WriteLine(@"   _____ __________ ");
                    Console.WriteLine(@"  /  _  \\______   \");
                    Console.WriteLine(@" /  /_\  \|     ___/");
                    Console.WriteLine(@"/    |    \    |    ");
                    Console.WriteLine(@"\____|__  /____|    ");
                    Console.WriteLine(@"        \/          ");
                    Console.WriteLine();
                    Console.WriteLine("Press Enter to exit...");
                    Console.ReadLine();
                }
                else 
                {
                    /*Limit of rudness of program right? hehe right xD machines are really very 
                     *rude they sometimes don't feel how bad a programmer feel when he encounter a unimaginary bug
                     */
                    return;
                }
            }
        }

        /// <summary>
        /// HTML ESCAPE string, well codechef returns me escaped strings; so my last task is to correct it
        /// You know what; I really think sometimes; why this is named as "escape" characters; Are they really escaping?
        /// If yes then see, i bounded them between curly brackets xD xD xD
        /// </summary>
        public static Dictionary<string, string> HTML_ESCAPE = new Dictionary<string, string>()
        {
            {"&gt;", ">"},
            {"&lt;", "<"},
            {"&amp;", "&"},
            {"<pre>", string.Empty},
            {"</pre>", string.Empty},
            {"&quot;", "\""},
        };

        /// <summary>
        /// This is a kinda many one mapping :D
        /// Same language has so many versions, i hate this :/
        /// But who cares, if i hate them or not :(
        /// </summary>
        public static Dictionary<string, string> LANGUAGE = new Dictionary<string, string>()
        {
            {"C++ 4.9.2", "cpp"},
            {"C++ 4.8.1", "cpp"},
            {"C++ 4.3.2", "cpp"},
            {"C++14", "cpp"},
            {"C++11", "cpp"},
            {"JAVA", "java"},//I seriously hate this language because only due to this language C# has no craze :( :'(
            {"C", "c"},
            {"C99 strict", "c"},
            {"C#", "cs"},
            {"F#", "fs"},
            {"PYTH", "py"},
            {"PYTH 3.1.2", "py"},
            {"ASM", "asm"},
            {"PHP", "php"},
            {"TEXT", "txt"},
            {"PERL", "pl"},
            {"JS", "js"}
        };
        public static void SaveCodeFile(string code, string Name, string lang)
        {
            Name = Name.Replace(@"status", string.Empty);
            var QualifiedPath = string.Format(@".\CodeChef\{0}.{1}", Name, LANGUAGE[lang]);

            //Yes, Bose i'll create new directory for each grouped questions; like contests
            //Cool na? Yea i'm very much cool
            Directory.CreateDirectory(Path.GetDirectoryName(QualifiedPath));
            var SW = File.CreateText(QualifiedPath);
            foreach (var Key in HTML_ESCAPE.Keys)
            {
                /*Now plz remove this HTML_ESCAPE strings; Else i'll kill you
                 *You know what i first time write it Name = code.replace xD
                 *And that time my code is not escaping strings in output code
                 *And i feel, hawww...Where is my mistake; why it is not giving proper code
                 *Finally i saw this; and laughed on myself xD
                 */
                code = code.Replace(Key, HTML_ESCAPE[Key]);
            }
            SW.WriteLine(code);
            SW.Flush();
            SW.Close();
        }

        /*
         * Hey Reader, Thank you for reading so much of code and my silly comments
         * At least i made you laugh more than once, if yes then laugh again :D
         * if no, then still you can laugh xD
         * Hopefully compiler won't read my comments; Else it will get crashed xD
         * 
         * NOTE: If you feel any grammetical error then go away,
         * Only me and my code knows my feeling, while writing these comments :D
         */
    }
}