using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvoiceCalculator;
using FakeItEasy;
using System.Collections.Generic;

namespace InvoiceCalculator.Test    
{
    [TestClass]
    public class InvoiceTest
    {
        //Unit test
        [TestMethod]
        public void CalculateTotalAmount_ShouldReturnTotalCharges_IfAccountExistsAndHasCharges()
        {      
            //Arrange      
            var dataProvider = A.Fake<IDataProvider>();           
            A.CallTo(() => dataProvider.GetCharges(A<int>.Ignored)).Returns(new List<decimal> {0m, 1m});
            var invoice = new Invoice(dataProvider);

            //Act
            var amount = invoice.CalculateTotalAmount(123);

            //Assert
            Assert.AreEqual(1m, amount);
        }
    }
}
