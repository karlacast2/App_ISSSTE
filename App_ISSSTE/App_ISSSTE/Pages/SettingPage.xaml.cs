﻿using Acr.UserDialogs;
using App_ISSSTE.Helpers;
using App_ISSSTE.Interfaces;
using App_ISSSTE.Models;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App_ISSSTE.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPage : ContentPage
    {
        
        public SettingPage()
        {
            InitializeComponent();
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                DisplayAlert("IMPORTANTE", "La aplicación esta sin internet, asi que solo funcionara con los datos almacenados previamente.", "Aceptar");
                App.Database.CargarInicial();
                this.LoadPacientes();
            }
            else
            {
                DisplayAlert("URGENTE", "Se recomienda sincronizar los datos", "Aceptar");
                App.Database.LoadPacientes();
                App.Database.CargarInicial();
                this.LoadPacientes();
            }
            MyListView.RefreshCommand = new Command(() =>
            {
                LoadRefreshing();
            });
            Buscador.IsVisible = false;
           
        }

        public async void LoadPacientes()
        {
            UserDialogs.Instance.ShowLoading("Cargando datos");
            await Task.Delay(1000);
            MyListView.ItemsSource = await App.Database.GetPeopleAsync();
            UserDialogs.Instance.HideLoading();
        }

        public async void LoadRefreshing()
        {
            UserDialogs.Instance.ShowLoading("Actulizando datos");
            await Task.Delay(1000);
            App.Database.LoadPacientes();
            App.Database.CargarInicial();
            MyListView.ItemsSource = await App.Database.GetPeopleAsync();
            MyListView.IsRefreshing = false;
            UserDialogs.Instance.HideLoading();
        }

        public async void HideSidePacientes()
        {
            UserDialogs.Instance.ShowLoading("Mostrando datos");
            await Task.Delay(100);
            MyListView.ItemsSource = await App.Database.GetPeopleAsync();
            UserDialogs.Instance.HideLoading();
        }

        public async void HideSidePacientesBus()
        {
            UserDialogs.Instance.ShowLoading("Mostrando datos");
            await Task.Delay(100);
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "control_issste1.db3");
            var db = new SQLiteConnection(databasePath);
            var keyWord = PacientesSearched.Text;
            if (keyWord.Length >= 1)
            {
                MyListView.IsVisible = false;
                Buscador.IsVisible = true;
                var get_docnumb = db.Query<Pacientes>("SELECT * FROM Pacientes WHERE nombre LIKE ?", "%" + PacientesSearched.Text + "%");
                Buscador.ItemsSource = get_docnumb;
            }
            else
            {
                Buscador.IsVisible = false;
                MyListView.IsVisible = true;
            }
            UserDialogs.Instance.HideLoading();
        }
        public async void Detalles(object sender, EventArgs e)
		{
            var buttonClickHandler = (Button)sender;
            Grid stackLayout = (Grid)buttonClickHandler.Parent;
            Label label = (Label)stackLayout.Children[0];
            string id = label.Text;
            Console.WriteLine("Holaaaaaa amor = " + id);
            await Navigation.PushAsync(new DatosPacientes(id));

        }

        private void MyListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var myselectitem = e.Item as Pacientes;
            App.Database.UpdatePacientes(myselectitem.id);
            HideSidePacientes();
        }

       public async void Handle_SearchButtonPressed(object sender, EventArgs e)
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "control_issste1.db3");
            var db = new SQLiteConnection(databasePath);
            SQLiteConnection connection = new SQLiteConnection(databasePath);
            var keyWord = PacientesSearched.Text;
            if (keyWord.Length >= 1)
            {
                MyListView.IsVisible = false;
                Buscador.IsVisible = true;
                var get_docnumb = db.Query<Pacientes>("SELECT * FROM Pacientes WHERE nombre LIKE ?", "%" + PacientesSearched.Text + "%");
                Buscador.ItemsSource =get_docnumb;
            }
            else
            {
                Buscador.IsVisible = false;
                MyListView.IsVisible = true;
            }
        }

        private void Buscador_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var myselectitem = e.Item as Pacientes;

            App.Database.UpdatePacientes(myselectitem.id);
            HideSidePacientesBus();
        }
    }
}