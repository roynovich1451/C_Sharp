﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ManageMovies
{
    /// <summary>
    /// Interaction logic for CollaborationMovieWindow.xaml
    /// </summary>
    public partial class CollaborationMovieWindow : Window
    {
        public CollaborationMovieWindow()
        {
            InitializeComponent();
            try
            {
                using (var ctx = new dbContext())
                {
                    var list = (from a in ctx.Actors
                                select a).ToList();
                    cmbFirstPar.ItemsSource = list;
                    cmbSecondPar.ItemsSource = list;
                }
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show($"No data found in DB about asked year", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Wrong string format entered, only numbers allowed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show(ex.Message + ", Iner: " + ex.InnerException.Message + "\n" + "Type: " + ex.GetType().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ", Iner: " + ex.InnerException.Message + "\n" + "Type: " + ex.GetType().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbFirstPar.SelectedItem == null || cmbSecondPar.SelectedItem == null)
                {
                    MessageBox.Show($"Must select two actors", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Actor firstActor = cmbFirstPar.SelectedItem as Actor;
                Actor secondActor = cmbSecondPar.SelectedItem as Actor;
                if (firstActor.Id == secondActor.Id)
                {
                    MessageBox.Show($"Must select two different actors", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                using (var ctx = new dbContext())
                {
                    //TODO continue from here
                    //query need to be changed
                    lbSearchResult.ItemsSource = (from fam in ctx.ActorMovie
                                                  where fam.ActorId == firstActor.Id
                                                  join sam in ctx.ActorMovie
                                                  on fam.MovieSerial equals sam.MovieSerial
                                                  join m in ctx.Movies
                                                  on fam.MovieSerial equals m.MovieSerial
                                                  select m
                                                  );
                }

            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show($"No data found in DB about asked year", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Wrong string format entered, only numbers allowed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show(ex.Message + ", Iner: " + ex.InnerException.Message + "\n" + "Type: " + ex.GetType().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ", Iner: " + ex.InnerException.Message + "\n" + "Type: " + ex.GetType().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
