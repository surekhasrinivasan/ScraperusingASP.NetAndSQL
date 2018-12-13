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
        private StockDatabaseEntities db = new StockDatabaseEntities();

        // GET: Stocks
        public ActionResult Index()
        {
            return View(db.Stocks.ToList());
        }

        // GET: Stocks/Details/5
        public ActionResult Details(string id)
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
        
        public ActionResult Scrape()
        {
            Scraper newScraper = new Scraper();
            var connString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=StockDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

                using (SqlConnection conn = new SqlConnection(connString))
                {
                conn.Open();
                     
                // Scraper run and save data to stockItems
                List<Stock> stockItems = newScraper.Scrape();
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
        public ActionResult Edit(string id)
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
        public ActionResult Edit([Bind(Include = "Symbol,LastPrice,Change,PercentChange,Currency,MarketTime,MarketCap")] Stock stock)
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
        public ActionResult Delete(string id)
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
        public ActionResult DeleteConfirmed(string id)
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
