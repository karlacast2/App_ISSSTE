using App_ISSSTE.Helpers;

using App_ISSSTE.Interfaces;

using App_ISSSTE.Pages;

using Newtonsoft.Json;

using SQLite;

using System;

using System.Collections;

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

        public List<Users> users;

        public ObservableCollection<Users> _posts1;

        public List<Municipio> municipios;

        public ObservableCollection<Municipio> _posts2;

        public Database(string dbPath)

        {

            //Establishing the conection 

            _database = new SQLiteAsyncConnection(dbPath);

            _database.CreateTableAsync<Pacientes>().ConfigureAwait(false);

            _database.CreateTableAsync<Pedidos>().ConfigureAwait(false);

            _database.CreateTableAsync<Users>().ConfigureAwait(false);

            _database.CreateTableAsync<Municipio>().ConfigureAwait(false);

        }



        // Show the registers 

        public Task<List<Pacientes>> GetPeopleAsync()

        {

            return _database.Table<Pacientes>().ToListAsync();

        }



        public Task<Pacientes> GetItemAsync(string Id)

        {

            return _database.Table<Pacientes>().Where(i => i.id == Id).FirstOrDefaultAsync();

        }



        // Save register 



        public async void LoadUser()

        {

            Console.WriteLine("Hola mi amor Te amo");

            //http://192.168.1.82:8000/ 

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Constants.BaseApiAddress);

            string url = string.Format("api/users");

            var response = await client.GetAsync(url);

            string result = response.Content.ReadAsStringAsync().Result;



            //await _database.DropTableAsync<Pacientes>().ConfigureAwait(false); 

            //await _database.CreateTableAsync<Pacientes>().ConfigureAwait(false); 

            this.users = JsonConvert.DeserializeObject<List<Users>>(result);

            _posts1 = new ObservableCollection<Users>(this.users);

            ///Select count(*) from Pacientes; = numero entero =10 

            var existuser = await _database.Table<Users>().CountAsync().ConfigureAwait(false);

            if (existuser != 0)

            {

                await InsertTable_User();

            }

            else

            {

                await _database.InsertAllAsync(_posts1).ConfigureAwait(false);

            }



        }



        public async void LoadPacientes()

        {

            //http://192.168.1.82:8000/ 

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Constants.BaseApiAddress);

            string url = string.Format("api/vista_paciente");

            var response = await client.GetAsync(url);

            string result = response.Content.ReadAsStringAsync().Result;



            //await _database.DropTableAsync<Pacientes>().ConfigureAwait(false); 

            //await _database.CreateTableAsync<Pacientes>().ConfigureAwait(false); 

            this.pacientes = JsonConvert.DeserializeObject<List<Pacientes>>(result);

            _posts = new ObservableCollection<Pacientes>(this.pacientes);

            ///Select count(*) from Pacientes; = numero entero =10 

            var existpaciente = await _database.Table<Pacientes>().CountAsync().ConfigureAwait(false);

            if (existpaciente != 0)

            {

                await InsertTable();

            }

            else

            {

                await _database.InsertAllAsync(_posts).ConfigureAwait(false);

            }



        }



        public async Task InsertTable_User()

        {

            for (int i = 0; i < _posts1.Count; i++)

            {



                using (await Mutex.LockAsync().ConfigureAwait(false))

                {

                    string Id = _posts1[i].Id_user;

                    //Select * from pacientes where id= 1 

                    var existuser = await _database.Table<Users>()
                            .Where(x => x.Id_user == Id)
                            .FirstOrDefaultAsync();

                    if (existuser == null)

                    {

                        await _database.InsertAsync(_posts1[i]).ConfigureAwait(false);

                    }

                    else

                    {

                        _posts1[i].Id_user = existuser.Id_user;

                        await _database.UpdateAllAsync(_posts1).ConfigureAwait(false);

                    }

                }

            }



        }



        public async Task InsertTable()

        {

            for (int i = 0; i < _posts.Count; i++)

            {



                using (await Mutex.LockAsync().ConfigureAwait(false))

                {

                    string id = _posts[i].id;

                    //Select * from pacientes where id= 1 

                    var existpacientes = await _database.Table<Pacientes>()

                            .Where(x => x.id == id)

                            .FirstOrDefaultAsync();

                    if (existpacientes == null)

                    {

                        await _database.InsertAsync(_posts[i]).ConfigureAwait(false);

                    }

                    else

                    {
                        _posts[i].id = existpacientes.id;
                        await _database.UpdateAllAsync(_posts).ConfigureAwait(false);
                    }
                }
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



        public Task<List<Municipio>> GetPeopleAsyncmuni()

        {

            return _database.Table<Municipio>().ToListAsync();

        }





        public async void Loadmuni()

        {

            Console.WriteLine("Hola mi amor Te amo");

            //http://192.168.1.82:8000/ 

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Constants.BaseApiAddress);

            string url = string.Format("api/vista_municipios");

            var response = await client.GetAsync(url);

            string result = response.Content.ReadAsStringAsync().Result;

            //await _database.DropTableAsync<Pacientes>().ConfigureAwait(false); 

            //await _database.CreateTableAsync<Pacientes>().ConfigureAwait(false); 

            this.Municipios = JsonConvert.DeserializeObject<List<Municipio>>(result);

            _posts2 = new ObservableCollection<Municipio>(this.Municipios);

            ///Select count(*) from Pacientes; = numero entero =10 
            ///
           await _database.InsertAllAsync(_posts2).ConfigureAwait(false);
            
        }
    }
}