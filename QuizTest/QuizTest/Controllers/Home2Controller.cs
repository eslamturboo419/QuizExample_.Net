using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using QuizTest.Models;

namespace QuizTest.Controllers
{
    public class Home2Controller : Controller
    {
        private QuizDBEntities db;

        public Home2Controller()
        {
            db = new QuizDBEntities();
        }



        // GET: Home2
        public ActionResult Index()
        {
            return View();
        }


        //// view all questions
        public ActionResult ViewQuestions()
        {
            var result = db.Questions.ToList();
            return View(result);
        }


        [HttpGet]
        public ActionResult AddQuestion()
        {
            ViewBag.Cat_Id = new SelectList(db.Categories.ToList(), "Id", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddQuestion(Question question)
        {
            if (ModelState.IsValid)
            {
                db.Questions.Add(question);
                db.SaveChanges();
                return RedirectToAction("ViewQuestions");
            }
            ViewBag.Cat_Id = new SelectList(db.Categories.ToList(), "Id", "Name", question.Cat_Id);
            return View(question);
        }





        public ActionResult ViewCatToShowQuestions()
        {
            TempData["listCat"] = db.Categories.Where(x => x.Id != 0).ToList();
            return View();
        }


        public ActionResult ToGetTempData(int Id)
        {
            List<Question> questions = db.Questions.Where(x => x.Cat_Id == Id).ToList();
            Queue<Question> queu = new Queue<Question>();

            foreach (Question a in questions)
            {
                queu.Enqueue(a);
            }
            
            TempData["questions"] = queu;
            TempData["score"] = 0;
            TempData.Keep();
            return RedirectToAction("QuestionsThisCategory");
        }




        public ActionResult QuestionsThisCategory()
        {
          

            Question q = null;
            if (TempData["questions"] != null)
            {
                Queue<Question> qlist = (Queue<Question>)TempData["questions"];
                int length = qlist.Count;
                if (qlist.Count > 0)
                {
                    q = qlist.Peek();
                    qlist.Dequeue();
                    TempData["questions"] = qlist;
                    TempData.Keep();

                    length = length - 1;
                }
                else
                {
                    return RedirectToAction("EndExam");
                }
                ViewBag.numberthis = length;
                ViewBag.total = qlist.Count;
            }
            else
            {
                return RedirectToAction("StudentExam");
            }

            return View(q);
        }

        [HttpPost]
        public ActionResult QuestionsThisCategory(Question q)
        {
            string correct = null;

            if (q.Answer_1 != null)
            {
                correct = "A";
            }
            else if (q.Answer_2 != null)
            {
                correct = "B";
            }
            else if (q.Answer_3 != null)
            {
                correct = "C";
            }
            else if (q.Answer_4 != null)
            {
                correct = "D";
            }

            if (correct.Equals(q.Answer_Correct))
            {
                TempData["score"] = Convert.ToInt32(TempData["score"]) + 1;
            }

            TempData.Keep();
            return RedirectToAction("QuestionsThisCategory");
        }




        public ActionResult EndExam()
        {
            return View();
        }

    }
}