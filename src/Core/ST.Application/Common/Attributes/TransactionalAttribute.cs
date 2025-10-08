using System;

namespace ST.Application.Common.Attributes
{
    /// <summary>
    /// Bu attribute ile işaretlenen bir CommandHandler'ın, bir veritabanı işlemi (transaction)
    /// içerisinde çalıştırılacağını belirtir.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TransactionalAttribute : Attribute
    {
    }
}