using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebScraperwithASP_NET.Models;

namespace WebScraperwithASP_NET.Controllers
{
    public class StocksController : Controller
    {
        private StockDatabaseEntities1 db = new StockDatabaseEntities1();

        // GET: Stocks
        public ActionResult Index()
        {
            return View(db.Stocks.ToList());
        }

        // GET: Stocks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        // GET: Stocks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Stocks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Id,Symbol,LastPrice,Change,PercentChange,Currency,MarketTime,MarketCap")] Stock stock)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Stocks.Add(stock);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(stock);
        //}

        // GET: Stocks/Scrape
        public ActionResult Scrape()
        {
            Scraper newScraper = new Scraper();

            // Scraper run and save data to stockItems
            List<Stock> stockItems = newScraper.Scrape();

            var connString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=StockDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            // Open SQL Connection
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // Move existing stock data to History table 
                SqlCommand moveData = new SqlCommand("INSERT INTO History SELECT * FROM Stock", conn);
                moveData.ExecuteNonQuery();
                db.SaveChanges();

                // Delete existing stock data and output new scraped data
                SqlCommand deleteAll = new SqlCommand("DELETE FROM Stock", conn);
                deleteAll.ExecuteNonQuery();
                db.SaveChanges();

                foreach (var stockItem in stockItems)
                {
                    stockItem.MarketTime = DateTime.Now;

                    SqlCommand insertCommand = new SqlCommand("INSERT INTO [Stock] (Symbol, LastPrice, Change, PercentChange, Currency, MarketCap) VALUES (@Symbol, @LastPrice, @Change, @PercentChange, @Currency, @MarketCap)", conn);
                    insertCommand.Parameters.AddWithValue("@Symbol", stockItem.Symbol.ToString());
                    insertCommand.Parameters.AddWithValue("@LastPrice", stockItem.LastPrice.ToString());
                    insertCommand.Parameters.AddWithValue("@Change", stockItem.Change.ToString());
                    insertCommand.Parameters.AddWithValue("@PercentChange", stockItem.PercentChange.ToString());
                    insertCommand.Parameters.AddWithValue("@Currency", stockItem.Currency.ToString());
                    //insertCommand.Parameters.AddWithValue("@MarketTime", stockItem.MarketTime);
                    insertCommand.Parameters.AddWithValue("@MarketCap", stockItem.MarketCap.ToString());
                    insertCommand.ExecuteNonQuery();
                }
                Console.WriteLine("Database Updated");
                conn.Close();
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Stocks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        // POST: Stocks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Symbol,LastPrice,Change,PercentChange,Currency,MarketTime,MarketCap")] Stock stock)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stock).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stock);
        }

        // GET: Stocks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        // POST: Stocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Stock stock = db.Stocks.Find(id);
            db.Stocks.Remove(stock);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
