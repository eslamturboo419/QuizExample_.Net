using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using PagedList;
using QuizTest.Models;

namespace QuizTest.Controllers
{
    public class HomeController : Controller
    {
        private QuizDBEntities db = new QuizDBEntities();

        public ActionResult Index()
        {
   
            if (Session["aa"] !=null)
            {
                return RedirectToAction("Dash");
            }
            return View();
        }


        // login admin
        [HttpGet]
        public ActionResult LogAdmin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LogAdmin(Admin admin)
        {
            var val = db.Admins.Where(x => x.Name == admin.Name && x.Password == admin.Password).FirstOrDefault();

            if (val !=null)
            {
                Session["aa"] = val.Id;
                return RedirectToAction("Dash");
            }

            return View(admin);
        }


        // register Now
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Student stu, HttpPostedFileBase Image)
        {
            string extention = "";
            
            if (ModelState.IsValid)
            {
                if (stu.Image != null)
                {
                         extention = Path.GetExtension( Image.FileName);
                        if (extention.ToLower().Equals(".jpg") || extention.ToLower().Equals(".png") || extention.ToLower().Equals(".jpeg"))
                        {
                        // save PDF in folder
                        string PDFName = Path.GetFileName(Image.FileName);
                        string PDFName2 = DateTime.Now.ToString("yymmss") + PDFName;
                        string physicalPath = Server.MapPath("~/ImgUpload/" + PDFName2);
                        Image.SaveAs(physicalPath);
                        stu.Image = PDFName2;
                         }
                    else
                    {
                        ModelState.AddModelError("ImgError", "Error");
                        return View(stu);
                    }       
                }
                else
                {
                    return View(stu);
                }


                Student student = new Student();
                student.Name= stu.Name;
                student.Password= stu.Password;
                student.Image = stu.Image;

                db.Students.Add(student);
                db.SaveChanges();
                return RedirectToAction("LogStudent");
            }
            return View(stu);
         }
        
        ////Upload Image 
        //public string UploadImage(HttpPostedFileBase Image )
        //{
        //    string path = "-1";

        //    if(Image !=null  && Image.ContentLength>0)
        //    {
        //        string extention = Path.GetExtension(Image.FileName);

        //        if (extention.ToLower().Equals("jpg")  || extention.ToLower().Equals("png") || extention.ToLower().Equals("jpeg"))
        //        {
        //            Random r = new Random();

        //            path = Path.Combine(Server.MapPath("~/ImgUpload"), r + Path.GetFileName(Image.FileName));

        //            Image.SaveAs(path);
        //            path = "~/ImgUpload" + r + Path.GetFileName(Image.FileName);
        //        }

        //    }
        //    return path;
        //}



        // dashBoard
        [HttpGet]
        public ActionResult Dash()
        {
            if (Session["aa"] == null)
            {
                return RedirectToAction("index");
            }

            return View();
        }


        // add Category

         [HttpGet]
        public ActionResult AddCategory()
        {
           if(Session["aa"] == null)
            {
                return RedirectToAction("index");
            }

            int y = Convert.ToInt32(Session["aa"].ToString());
            ViewData["List"] =  db.Categories.Where(x=>x.Admin_Id ==y )  // where this is add it
                .OrderByDescending(x => x.Id).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult AddCategory(Category model)
        {

            ViewData["List"] = db.Categories.OrderByDescending(x => x.Id).ToList();

            Random r = new Random();
            Category c = new Category();
            c.Name = model.Name;

            c.EncRoom = CtyEncrpt.Encrypt(model.Name.Trim() + r.Next().ToString() ,true );

            c.Admin_Id =  Convert.ToInt32(Session["aa"].ToString() )  ;

            db.Categories.Add(c);
            db.SaveChanges();
            return RedirectToAction("AddCategory");
        }


        // view  All This Question
        //id
        public ActionResult ViewAllQuestion(int ?id , int ? Page)
        {
            if (Session["aa"] == null)
            {
                return RedirectToAction("LogAdmin");
            }

            if(id==null)
            {
                return RedirectToAction("Dash");
            }
            return View(db.Questions.Where(x=>x.Cat_Id==id).ToList().ToPagedList(Page ?? 1, 3));
        }



        // add Question

        [HttpGet]
        public ActionResult AddQuestion()
        {
            if (Session["aa"] == null)
            {
                return RedirectToAction("index");
            }

            ViewBag.Cat_Id = new SelectList(db.Categories.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult AddQuestion(Question model)
        {
            ViewBag.Cat_Id = new SelectList(db.Categories.ToList(), "Id", "Name");

            if(ModelState.IsValid)
            {
                Question q = new Question();
                q.Qu_Name = model.Qu_Name;
                q.Answer_1 = model.Answer_1;
                q.Answer_2 = model.Answer_2;
                q.Answer_3 = model.Answer_3;
                q.Answer_4 = model.Answer_4;
                q.Answer_Correct = model.Answer_Correct;
                q.Cat_Id = model.Cat_Id;

                db.Questions.Add(q);
                db.SaveChanges();
                return RedirectToAction("Dash");
            }

            return View(model);
        }




        // login Student
        [HttpGet]
        public ActionResult LogStudent()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogStudent(Student stu)
        {
            var val = db.Students.Where(x => x.Name == stu.Name && x.Password == stu.Password).FirstOrDefault();

            if(val !=null)
            {
                Session["Student"] = val.Id;
                return RedirectToAction("StudentExam");
            }

            return View(stu);
        }

        public ActionResult StudentExam()
        {
            if(Session["Student"] ==null)
            {
                return RedirectToAction("LogStudent");
            }
            return View();
        }

        [HttpPost]
        public ActionResult StudentExam(string room)
        {
            List<Category> cat = db.Categories.ToList();

            foreach (var item in cat)
            {
                if(item.EncRoom==room)
                {
                    List<Question> questions = db.Questions.Where(x => x.Cat_Id == item.Id).ToList();
                    Queue<Question> queu = new Queue<Question>();

                    foreach (Question a in questions)
                    {
                        queu.Enqueue(a);
                    }

                    // TempData["empId"]=item.Id;
                    TempData["questions"] = queu;
                    TempData["score"] = 0;
                    TempData.Keep();
                    return RedirectToAction("QuizStart");
                }
                else
                {
                    ViewBag.error = "No Room Found .........";
                }
            }


            return View();
        }

        //if (Session["Student"] == null)
        //{
        //    return RedirectToAction("LogStudent");
        //}

        public ActionResult QuizStart()
        {
            Question q = null;
            if(TempData["questions"] !=null)
            {
                Queue<Question> qlist = (Queue<Question>)TempData["questions"];
                int length = qlist.Count;
                if(qlist.Count>0)
                {
                    q = qlist.Peek();
                    qlist.Dequeue();
                    TempData["questions"] = qlist;
                    TempData.Keep();

                     length = length-1;
                }
                else
                {
                    return RedirectToAction("EndExam");
                }
               // ViewBag.listOfQuque = length;
                ViewBag.numberthis = length ;
            }
            else
            {
                return RedirectToAction("StudentExam");
            }
            
            return View(q);
        }




        //[HttpGet]
        //public ActionResult QuizStart()
        //{
        //    if (TempData["i"] == null)
        //    {
        //        TempData["i"] = 1;
        //    }

        //    if (Session["Student"] == null)
        //    {
        //        return RedirectToAction("LogStudent");
        //    }

        //    try
        //    {
        //        Question Data = null;
        //        int emamid = Convert.ToInt32(TempData["empId"].ToString());

        //        if (TempData["qid"] == null)
        //        {
        //            Data = db.Questions.Where(x => x.Cat_Id == emamid).FirstOrDefault();


        //            var list = db.Questions.Skip(Convert.ToInt32(TempData["i"].ToString()));
        //            int qid = list.First().Id;

        //            TempData["qid"] = ++Data.Id;
        //        }
        //        else
        //        {
        //            int quesId = Convert.ToInt32(TempData["qid"].ToString());
        //            Data = db.Questions.Where(x => x.Id == quesId && x.Cat_Id == emamid).FirstOrDefault();

        //            var list = db.Questions.Skip(Convert.ToInt32(TempData["i"].ToString()));
        //            int  qid = list.First().Id;
        //            TempData["qid"] = qid;
        //            TempData["i"] = Convert.ToInt32(TempData["i"].ToString()) + 1;

        //        }
        //        TempData.Keep();
        //        return View(Data);
        //    }
        //    catch (Exception)
        //    {

        //        return RedirectToAction("StudentExam");
        //    }

        //}

        [HttpPost]
        public ActionResult QuizStart(Question q)
        {
            string correct = null;

            if (q.Answer_1 !=null)
            {
                //if(q.Answer_1.Equals(q.Answer_Correct) )
                //{
                //    TempData["score"] =Convert.ToInt32( TempData["score"].ToString() ) + 1;
                //}

                correct = "A";
            }
           else if (q.Answer_2 != null)
            {
                //if (q.Answer_2.Equals(q.Answer_Correct))
                //{
                //    TempData["score"] = Convert.ToInt32(TempData["score"].ToString()) + 1;
                //}

                correct = "B";
            }
            else if (q.Answer_3 != null)
            {
                //if (q.Answer_3.Equals(q.Answer_Correct))
                //{
                //    TempData["score"] = Convert.ToInt32(TempData["score"].ToString()) + 1;
                //}
                correct = "C";

            }
            else if (q.Answer_4 != null)
            {
                //if (q.Answer_4.Equals(q.Answer_Correct))
                //{
                //    TempData["score"] = Convert.ToInt32(TempData["score"].ToString()) + 1;
                //}
                correct = "D";
            }

            if (correct.Equals(q.Answer_Correct))
            {
                TempData["score"] = Convert.ToInt32(TempData["score"]) + 1;
            }

            TempData.Keep();
            return RedirectToAction("QuizStart");
        }

        public ActionResult EndExam()
        {
            return View();
        }




        // log out
        public ActionResult LogOut()
        {
            Session.Abandon();
            Session.RemoveAll();
            Thread.Sleep(500);
            return RedirectToAction("Index");
        }



    }
}