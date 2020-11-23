using EmployeesDb.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EmployeesDB
{
	class Program
	{
		private static readonly EmployeesContext context = new EmployeesContext();

		static void Main(string[] args)
		{
			Task_1_SQL();
		}

		static void Task_1()
		{
			var employees = context.Employees
				.Where(e => e.Salary > 48000)
				.OrderBy(e => e.LastName)
				.Include(e => e.Department)
				.Include(e => e.Address)
				.Select(e => new
				{
					Name = e.FirstName + " " + e.LastName,
					e.MiddleName,
					e.JobTitle,
					e.HireDate,
					e.Salary,
					Address = e.Address.AddressText,
					Departament = e.Department.Name,
					Manager = e.Department.Manager.FirstName + " " + e.Department.Manager.LastName
				})
				.ToList();

			foreach (var em in employees)
				Console.WriteLine("{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7}",
					em.Name, em.MiddleName, em.Address, em.Departament, em.Manager, em.JobTitle, em.HireDate, em.Salary);
		}

		static void Task_1_SQL()
		{
			string nativeSqlQuery = $"SELECT * FROM Employees AS e " +
									$"WHERE e.Salary > 48000";

			var employees = context.Employees.FromSqlRaw(nativeSqlQuery)
				.Include(e => e.Department)
				.Include(e => e.Address)
				.OrderBy(e => e.LastName)
				.ToList();

			foreach (var em in employees)
				Console.WriteLine("{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7}",
					em.FirstName + " " + em.LastName, em.MiddleName, em.Address.AddressText, em.Department.Name,
					em.Department.Manager.FirstName + " " + em.Department.Manager.LastName, em.JobTitle, em.HireDate, em.Salary);
		}

		static void Task_2()
		{
			Console.Write("Введите город: ");
			string towns = Console.ReadLine();
			Console.Write("Введите адрес: ");
			var newAddress = Console.ReadLine();

			var selectTown = context.Towns.Where(t => t.Name.Contains(towns)).FirstOrDefault();

			if (selectTown != null)
			{
				Addresses addAddress = new Addresses()
				{
					AddressText = newAddress,
					TownId = selectTown.TownId
				};

				context.Addresses.Add(addAddress);

				try
				{
					context.SaveChanges();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.InnerException.Message);
				}

				UpdateUsers(newAddress);
			}
			else
			{
				Console.WriteLine("Данного города не существует в базе, желаете добавить?");
				Console.Write("Введите Y/N (Да/Нет): ");
				string choose = Console.ReadLine();

				switch (choose)
				{
					case "Y":
						Towns addTowns = new Towns()
						{
							Name = towns
						};

						context.Towns.Add(addTowns);

						try
						{
							context.SaveChanges();
						}
						catch (Exception e)
						{
							Console.WriteLine(e.InnerException.Message);
							break;
						}

						var twId = context.Towns.Where(t => t.Name.Contains(towns)).FirstOrDefault();

						if (twId == null)
						{
							Console.WriteLine("Не удалось добавить город, повторите попытку позже..");
							break;
						}
						else
						{
							Addresses addAddressWithTown = new Addresses()
							{
								AddressText = newAddress,
								TownId = twId.TownId
							};

							context.Addresses.Add(addAddressWithTown);

							try
							{
								context.SaveChanges();
							}
							catch (Exception e)
							{
								Console.WriteLine(e.InnerException.Message);
								break;
							}

							UpdateUsers(newAddress);
						}

						break;

					case "N":
						Console.WriteLine("\nЗавершение работы..");
						break;
				}
			}
		}

		static void Task_2_SQL()
		{
			Console.Write("Введите город: ");
			string towns = Console.ReadLine();
			Console.Write("Введите адрес: ");
			var newAddress = Console.ReadLine();

			string nativeSqlQuery = $"SELECT * FROM Towns AS t " +
									$"WHERE t.Name = @town";
			SqlParameter param = new SqlParameter("@town", towns);

			var selectTown = context.Towns.FromSqlRaw(nativeSqlQuery, param).FirstOrDefault();

			if (selectTown != null)
			{
				Addresses addAddress = new Addresses()
				{
					AddressText = newAddress,
					TownId = selectTown.TownId
				};

				context.Addresses.Add(addAddress);

				try
				{
					context.SaveChanges();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.InnerException.Message);
				}

				UpdateUsers(newAddress);
			}
			else
			{
				Console.WriteLine("Данного города не существует в базе, желаете добавить?");
				Console.Write("Введите Y/N (Да/Нет): ");
				string choose = Console.ReadLine();

				switch (choose)
				{
					case "Y":
						Towns addTowns = new Towns()
						{
							Name = towns
						};

						context.Towns.Add(addTowns);

						try
						{
							context.SaveChanges();
						}
						catch (Exception e)
						{
							Console.WriteLine(e.InnerException.Message);
							break;
						}

						string nativeTwId = $"SELECT * FROM Towns AS t " +
											$"WHERE t.Name = @town";
						SqlParameter paramTwId = new SqlParameter("@town", towns);

						var twId = context.Towns.FromSqlRaw(nativeTwId, paramTwId).FirstOrDefault();

						if (twId == null)
						{
							Console.WriteLine("Не удалось добавить город, повторите попытку позже..");
							break;
						}
						else
						{
							Addresses addAddressWithTown = new Addresses()
							{
								AddressText = newAddress,
								TownId = twId.TownId
							};

							context.Addresses.Add(addAddressWithTown);

							try
							{
								context.SaveChanges();
							}
							catch (Exception e)
							{
								Console.WriteLine(e.InnerException.Message);
								break;
							}

							UpdateUsers_SQL(newAddress);
						}

						break;

					case "N":
						Console.WriteLine("\nЗавершение работы..");
						break;
				}
			}
		}

		static void UpdateUsers(string address)
		{
			var employees = context.Employees.Where(e => e.LastName.Contains("Brown"));
			var addresses = context.Addresses.Where(ad => ad.AddressText.Contains(address)).FirstOrDefault();

			foreach (var em in employees)
				em.AddressId = addresses.AddressId;

			try
			{
				context.SaveChanges();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.InnerException.Message);
			}

			var employeesNew = context.Employees
				.Include(e => e.Department)
				.Include(e => e.Address)
				.Where(e => e.LastName.Contains("Brown"))
				.Select(e => new
				{
					BaseInfo = e.FirstName + " " + e.LastName + " " + e.JobTitle + " " + e.HireDate + " " + e.Salary,
					Manager = e.Department.Manager.FirstName + " " + e.Department.Manager.LastName,
					Address = e.Address.AddressText
				})
				.ToList();

			foreach (var em in employeesNew)
				Console.WriteLine("{0} \t{1} \t{2}", em.BaseInfo, em.Manager, em.Address);
		}

		static void UpdateUsers_SQL(string address)
		{
			string nativeEmploees = $"SELECT * FROM Employees AS e " +
									$"WHERE e.Name = @name";
			SqlParameter paramEmployees = new SqlParameter("@name", "Brown");
			var employees = context.Employees.FromSqlRaw(nativeEmploees, paramEmployees);

			string nativeAddress = $"SELECT * FROM Addresses AS a " +
									$"WHERE a.AddressText = @address";
			SqlParameter paramAddress = new SqlParameter("@address", address);
			var addresses = context.Addresses.FromSqlRaw(nativeAddress, paramAddress).FirstOrDefault();

			foreach (var em in employees)
				em.AddressId = addresses.AddressId;

			try
			{
				context.SaveChanges();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.InnerException.Message);
			}

			string nativeEmployeesNew = $"SELECT * FROM Employees AS e " +
										$"WHERE e.LastName = @name";
			SqlParameter paramEmployeesNew = new SqlParameter("@name", "Brown");
			var employeesNew = context.Employees
				.FromSqlRaw(nativeEmployeesNew, paramEmployeesNew)
				.Include(e => e.Address)
				.Include(e => e.Department)
				.ToList();

			foreach (var em in employeesNew)
				Console.WriteLine("{0} \t{1} \t{2}", em.FirstName + " " + em.LastName + " " + em.JobTitle + " " + em.HireDate + " " + em.Salary,
					em.Department.Manager.FirstName + " " + em.Department.Manager.LastName,
					em.Address.AddressText);
		}

		static void Task_3()
		{
			DateTime dateFirst = new DateTime(2002, 1, 1);
			DateTime dateLast = new DateTime(2005, 12, 31);

			var employees = context.Employees
				.Include(e => e.EmployeesProjects)
					.ThenInclude(em => em.Project)
				.Select(e => new
				{
					Name = e.FirstName + " " + e.LastName,
					Manager = e.Department.Manager.FirstName + " " + e.Department.Manager.LastName,
					ProjectName = e.EmployeesProjects.Select(p => p.Project.Name).FirstOrDefault(),
					ProjectStart = e.EmployeesProjects.Select(p => p.Project.StartDate).FirstOrDefault(),
					ProjectEnd = e.EmployeesProjects.Select(p => p.Project.EndDate).FirstOrDefault(),
					Message = !e.EmployeesProjects.Select(p => p.Project.EndDate).FirstOrDefault().HasValue ? "Проект не завершен" : null
				})
				.Where(e => e.ProjectStart >= dateFirst && e.ProjectStart <= dateLast)
				.Take(5)
				.ToList();

			foreach (var em in employees)
			{
				Console.WriteLine("Employee: {0} | Manager: {1}", em.Name, em.Manager);
				Console.WriteLine("Project: {0} | Start: {1} | End: {2}{3}", em.ProjectName, em.ProjectStart,
					em.ProjectEnd, em.Message);
				Console.WriteLine();
			}
		}

		static void Task_3_SQL()
		{
			DateTime dateFirst = new DateTime(2002, 1, 1);
			DateTime dateLast = new DateTime(2005, 12, 31);

			string nativeSqlQuery = $"SELECT * FROM Employees AS e";

			var employees = context.Employees
				.FromSqlRaw(nativeSqlQuery)
				.Include(e => e.EmployeesProjects)
					.ThenInclude(ee => ee.Project)
				.Include(e => e.Department)
				.Where(e => e.EmployeesProjects.Select(p => p.Project.StartDate).FirstOrDefault() >= dateFirst &&
					e.EmployeesProjects.Select(p => p.Project.StartDate).FirstOrDefault() <= dateLast)
				.Take(5)
				.ToList();

			foreach (var em in employees)
			{
				Console.WriteLine("Employee: {0} | Manager: {1}", em.FirstName + " " + em.LastName, em.Department.Manager.FirstName + " " + em.Department.Manager.LastName);
				Console.WriteLine("Project: {0} | Start: {1} | End: {2}{3}",
					em.EmployeesProjects.Select(p => p.Project.Name).FirstOrDefault(),
					em.EmployeesProjects.Select(p => p.Project.StartDate).FirstOrDefault(),
					em.EmployeesProjects.Select(p => p.Project.EndDate).FirstOrDefault(),
					em.EmployeesProjects.Select(p => p.Project.EndDate).FirstOrDefault().HasValue ? null : "Проект не завершен");
				Console.WriteLine();
			}
		}

		static void Task_4()
		{
			Console.Write("Введите ID сотрудника: ");
			int id = Int32.Parse(Console.ReadLine());

			var employee = context.Employees
				.Include(e => e.EmployeesProjects)
					.ThenInclude(em => em.Project)
				.Where(e => e.EmployeeId == id)
				.Select(e => new
				{
					Name = e.FirstName + " " + e.LastName,
					e.MiddleName,
					e.JobTitle,
					Project = e.EmployeesProjects.Select(p => p.Project).ToList()
				})
				.ToList();

			foreach (var em in employee)
			{
				Console.WriteLine("{0} {1} - {2}", em.Name, em.MiddleName, em.JobTitle);

				foreach (var p in em.Project)
					Console.WriteLine("	{0}", p.Name);

				Console.WriteLine();
			}
		}

		static void Task_4_SQL()
		{
			Console.Write("Введите ID сотрудника: ");
			int id = Int32.Parse(Console.ReadLine());

			string nativeSqlQuery = $"SELECT * FROM Employees AS e " +
				$"WHERE e.EmployeeID = @id";
			SqlParameter paramId = new SqlParameter("@id", id);

			var employee = context.Employees
				.FromSqlRaw(nativeSqlQuery, paramId)
				.Include(e => e.EmployeesProjects)
					.ThenInclude(em => em.Project)
				.ToList();

			foreach (var em in employee)
			{
				Console.WriteLine("{0} {1} - {2}", em.FirstName + " " + em.LastName, em.MiddleName, em.JobTitle);

				foreach (var p in em.EmployeesProjects.Select(p => p.Project).ToList())
					Console.WriteLine("	{0}", p.Name);

				Console.WriteLine();
			}
		}

		static void Task_5()
		{
			var departments = context.Employees
				.Include(d => d.Department)
				.GroupBy(d => new
				{
					d.Department.Name,
					d.DepartmentId
				})
				.Select(c => new
				{
					Count = c.Count(),
					DepName = c.Key.Name
				})
				.Where(c => c.Count <= 5);


			foreach (var d in departments)
				Console.WriteLine("Отдел: {0} | Кол-во сотрудников: {1}", d.DepName, d.Count);
		}

		static void Task_5_SQL()
		{
			string nativeSqlQuery = $"SELECT * FROM Employees AS e"; ;

			var departments = context.Employees
				.FromSqlRaw(nativeSqlQuery)
				.Include(d => d.Department)
				.ToList();

			foreach (var d in departments.GroupBy(d => new { d.DepartmentId, d.Department.Name }).Where(c => c.Count() <= 5))
				Console.WriteLine("Отдел: {0} | Кол-во сотрудников: {1} ", d.Key.Name, d.Count());
		}

		static void Task_6()
		{
			Console.Write("Введите название отдела: ");
			string department = Console.ReadLine();
			Console.Write("Введите кол-во процентов: ");
			int percent = Int32.Parse(Console.ReadLine());
			Console.WriteLine();

			var employees = context.Employees
				.Include(e => e.Department)
				.Where(e => e.Department.Name.Contains(department));

			if (employees.Count() == 0)
			{
				Console.WriteLine("Данного отдела не существует или в нём нет сотрудников..");
				return;
			}
			else
			{
				foreach (var em in employees)
					em.Salary += em.Salary * percent / 100;

				try
				{
					context.SaveChanges();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.InnerException.Message);
				}

				var employees_new = context.Employees
					.Include(e => e.Department)
					.Where(e => e.Department.Name.Contains(department))
					.Select(e => new
					{
						e.Salary
					})
					.ToList();

				Console.WriteLine("Новая зарплата сотрудников:");

				foreach (var e in employees)
					Console.WriteLine("	{0}", e.Salary);
			}
		}

		static void Task_6_SQL()
		{
			Console.Write("Введите название отдела: ");
			string department = Console.ReadLine();
			Console.Write("Введите кол-во процентов: ");
			int percent = Int32.Parse(Console.ReadLine());
			Console.WriteLine();

			string nativeSqlQuery = $"SELECT * FROM Employees AS e";

			var employees = context.Employees.FromSqlRaw(nativeSqlQuery)
				.Include(e => e.Department)
				.Where(e => e.Department.Name == department);

			if (employees.Count() == 0)
			{
				Console.WriteLine("Данного отдела не существует или в нём нет сотрудников..");
				return;
			}
			else
			{
				foreach (var em in employees)
					em.Salary += em.Salary * percent / 100;

				try
				{
					context.SaveChanges();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.InnerException.Message);
				}

				string nativeSqlQuery_new = $"SELECT * FROM Employees AS e";

				var employees_new = context.Employees
					.FromSqlRaw(nativeSqlQuery_new)
					.Include(e => e.Department)
					.Where(e => e.Department.Name == department)
					.ToList();

				Console.WriteLine("Новая зарплата сотрудников:");

				foreach (var e in employees)
					Console.WriteLine("	{0}", e.Salary);
			}
		}

		static void Task_7()
		{
			Console.Write("Введите ID отдела, который желаете удалить: ");
			int depId = Int32.Parse(Console.ReadLine());

			var department = context.Departments
				.Where(d => d.DepartmentId == depId)
				.FirstOrDefault();

			if (department == null)
			{
				Console.WriteLine("Данного отдела не существует, повторите попытку..");
				return;
			}
			else
			{
				context.Departments.Remove(department);

				try
				{
					context.SaveChanges();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.InnerException.Message);
					return;
				}
			}
		}

		static void Task_7_SQL()
		{
			Console.Write("Введите ID отдела, который желаете удалить: ");
			int depId = Int32.Parse(Console.ReadLine());

			string nativeSqlQuery = $"SELECT * FROM Departments AS d " +
				$"WHERE d.DepartmentID = @id";
			SqlParameter paramId = new SqlParameter("@id", depId);

			var department = context.Departments
				.FromSqlRaw(nativeSqlQuery, paramId)
				.FirstOrDefault();

			if (department == null)
			{
				Console.WriteLine("Данного отдела не существует, повторите попытку..");
				return;
			}
			else
			{
				context.Departments.Remove(department);

				try
				{
					context.SaveChanges();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.InnerException.Message);
					return;
				}
			}
		}

		static void Task_8()
		{
			Console.Write("Введите город, который хотите удалить: ");
			string town = Console.ReadLine();

			var townDb = context.Towns
				.Where(t => t.Name.Contains(town))
				.FirstOrDefault();

			if (townDb == null)
			{
				Console.WriteLine("Данного города не существует, повторите попытку..");
				return;
			}
			else
			{
				context.Entry(townDb).Collection(c => c.Addresses).Load();
				context.Towns.Remove(townDb);

				try
				{
					context.SaveChanges();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.InnerException.Message);
					return;
				}

				Console.WriteLine("Город {0} был удалён.", town);
			}
		}

		static void Task_8_SQL()
		{
			Console.Write("Введите город, который хотите удалить: ");
			string town = Console.ReadLine();

			string nativeSqlQuery = $"SELECT * FROM Towns AS t " +
				$"WHERE t.Name = @town";
			SqlParameter param = new SqlParameter("@town", town);

			var townDb = context.Towns
				.FromSqlRaw(nativeSqlQuery, param)
				.FirstOrDefault();

			if (townDb == null)
			{
				Console.WriteLine("Данного города не существует, повторите попытку..");
				return;
			}
			else
			{
				context.Entry(townDb).Collection(c => c.Addresses).Load();
				context.Towns.Remove(townDb);

				try
				{
					context.SaveChanges();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.InnerException.Message);
					return;
				}

				Console.WriteLine("Город {0} был удалён.", town);
			}
		}
	}
}
