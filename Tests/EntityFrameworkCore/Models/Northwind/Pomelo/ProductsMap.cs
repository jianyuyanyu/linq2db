﻿using LinqToDB.EntityFrameworkCore.Tests.Models.Northwind;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinqToDB.EntityFrameworkCore.Tests.Pomelo.Models.Northwind.Mapping
{
	public class ProductsMap : IEntityTypeConfiguration<Product>
	{
		public void Configure(EntityTypeBuilder<Product> builder)
		{
			builder.HasKey(e => e.ProductId);

			builder.HasIndex(e => e.CategoryId)
				.HasDatabaseName("CategoryID");

			builder.HasIndex(e => e.ProductName)
				.HasDatabaseName("ProductName");

			builder.HasIndex(e => e.SupplierId)
				.HasDatabaseName("SuppliersProducts");

			builder.Property(e => e.ProductId).HasColumnName("ProductID")
				.ValueGeneratedNever();

			builder.Property(e => e.CategoryId).HasColumnName("CategoryID");

			builder.Property(e => e.ProductName)
				.IsRequired()
				.HasMaxLength(40);

			builder.Property(e => e.QuantityPerUnit).HasMaxLength(20);

			builder.Property(e => e.ReorderLevel).HasDefaultValue((short)0);

			builder.Property(e => e.SupplierId).HasColumnName("SupplierID");

			builder.Property(e => e.UnitPrice)
				.HasColumnType("decimal(13, 4)")
				.HasDefaultValue(0m);

			builder.Property(e => e.UnitsInStock).HasDefaultValue((short)0);

			builder.Property(e => e.UnitsOnOrder).HasDefaultValue((short)0);

			builder.HasOne(d => d.Category!)
				.WithMany(p => p.Products)
				.HasForeignKey(d => d.CategoryId)
				.HasConstraintName("FK_Products_Categories");

			builder.HasOne(d => d.Supplier!)
				.WithMany(p => p.Products)
				.HasForeignKey(d => d.SupplierId)
				.HasConstraintName("FK_Products_Suppliers");
		}
	}
}
