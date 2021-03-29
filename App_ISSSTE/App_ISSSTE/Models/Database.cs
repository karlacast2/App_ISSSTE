﻿using App_ISSSTE.Helpers;
using App_ISSSTE.Interfaces;
using App_ISSSTE.Pages;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App_ISSSTE.Models
{
    public class Database
    {
        private static readonly AsyncLock Mutex = new AsyncLock();
        readonly SQLiteAsyncConnection _database;
        public List<Pacientes> pacientes;
        public ObservableCollection<Pacientes> _posts;
        public Database(string dbPath)
        {
            //Establishing the conection
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Pacientes>().ConfigureAwait(false);
            _database.CreateTableAsync<Pedidos>().ConfigureAwait(false);
            _database.DropTableAsync<Users>().ConfigureAwait(false);
            _database.CreateTableAsync<Users>().ConfigureAwait(false);
        }

        // Show the registers
        public Task<List<Pacientes>> GetPeopleAsync()
        {
            return _database.Table<Pacientes>().ToListAsync();
        }

        public Task<Pacientes> GetItemAsync(string Id)
        {
            return _database.Table<Pacientes>().Where(i => i.id ==Id).FirstOrDefaultAsync();
        }

        // Save register
        public async void LoadPacientes()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                
            }
            else
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(Constants.BaseApiAddress);
                string url = string.Format("api/vista_paciente");
                var response = await client.GetAsync(url);
                string result = response.Content.ReadAsStringAsync().Result;
                

                //await _database.DropTableAsync<Pacientes>().ConfigureAwait(false);
                //await _database.CreateTableAsync<Pacientes>().ConfigureAwait(false);
                this.pacientes = JsonConvert.DeserializeObject<List<Pacientes>>(result);
                _posts = new ObservableCollection<Pacientes>(this.pacientes);
                await _database.UpdateAllAsync(_posts).ConfigureAwait(false);
            }
            
        }

        public async void UpdatePacientes(string sd)
        {
            var pacientes = await App.Database.GetItemAsync(sd);
            if (pacientes.IsVisibl == "False")
            {
                await _database.QueryAsync<Pacientes>("UPDATE Pacientes SET IsVisibl = 'False'" + "WHERE IsVisibl = 'True'");
                await _database.QueryAsync<Pacientes>("UPDATE Pacientes SET IsVisibl = 'True'" + "WHERE id = ?", sd);
            }
            else
            {
                if (pacientes.IsVisibl == "True")
                {
                    await _database.QueryAsync<Pacientes>("UPDATE Pacientes SET IsVisibl = 'False'" + "WHERE IsVisibl = 'True'");
                }
            }
        }

        public async void CargarInicial()
        {
            await _database.QueryAsync<Pacientes>("UPDATE Pacientes SET IsVisibl = 'False'" + "WHERE IsVisibl = 'True'");
        }
        // Delete registers
        public Task<int> DeletePersonAsync(Pacientes paciente)
        {
            return _database.DeleteAsync(paciente);
        }

        // Save registers
        public Task<int> UpdatePersonAsync(Pacientes pacientes)
        {
            return _database.UpdateAsync(pacientes);
        }

        public async Task<string> AddUser(Users user)
        {
            using (await Mutex.LockAsync().ConfigureAwait(false))
            {
                
                var existingTodoItem = await _database.Table<Users>()
                        .Where(x => x.Email == user.Email)
                        .FirstOrDefaultAsync();


                if (existingTodoItem == null)
                {
                    await _database.InsertAsync(user);
                    return "Añadido exitosamente";
                }
                else
                {

                    return "Ya existe el mismo correo";
                }
            }
        }

        public async Task<bool> LoginValidate(string userName1, string pwd1)
        {
            using (await Mutex.LockAsync().ConfigureAwait(false))
            {
                var existingTodoItem = await _database.Table<Users>()
                        .Where(x => x.Email == userName1 && x.Password == pwd1)
                        .FirstOrDefaultAsync();

                if (existingTodoItem != null)
                {
                    return true;
                }
                else
                    return false;
            }
        }
    }
}
