﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ooadproject.Models
{
    public class StudentExam : IActivity, INotificationObservable
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("StudentCourse")]
        public int CourseID { get; set; }
        public StudentCourse? Course { get; set; }

        [ForeignKey("Exam")]
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }  

        public double PointsScored { get; set; }
        public bool IsPassed { get; set; }

        private NotificationManager notification;

        public StudentExam(StudentCourse course, Exam exam, double points, bool isPassed, NotificationManager notif) 
        {
            this.Course = course;
            this.Exam = exam;
            this.PointsScored = points;
            this.IsPassed = isPassed;
            this.notification = notif;
            this.Notify();
        }

        public double GetPointsScored()
        {
            return PointsScored;
        }

        public double GetTotalPoints()
        {
            return Exam.TotalPoints;
        }

        public DateTime GetActivityDate()
        {
            return Exam.Time.Date;
        }

        public string GetActivityName()
        {
            throw new System.NotImplementedException();
        }

        public string GetActivityType()
        {
            return Exam.Type.ToString();
        }

        public void Attach(NotificationManager notifications)
        {
            this.notification = notification;
        }

        public void Detach()
        {
            notification = null;
        }

        public void Notify()
        {
            notification.UpdateForExamResoults(this);
        }
    }
}
