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
        [Authorize]
        public ActionResult Index()
        {
            return View(db.Stocks.ToList());
        }
        
        // GET: Stocks/Scrape
        [Authorize]
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

        // GET: Stocks/History
        [Authorize]
        public ActionResult History()
        {
            return View(db.Histories.ToList());
        }        
    }
}