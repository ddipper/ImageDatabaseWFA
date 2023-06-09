using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageDatabaseWFA
{
    public partial class ImageToDataBase : Form
    {
        private string con = @"Data Source=LENOVO_S340\SQLEXPRESS;Initial Catalog=ImageSelectDB;Integrated Security=True";

        public ImageToDataBase()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (openFileDialog.ShowDialog() == DialogResult.OK) pictureSet.Image = Image.FromFile(openFileDialog.FileName);
            }
        }

        class ImageUploader
        {
            private readonly string _connectionString;

            public ImageUploader(string connectionString)
            {
                _connectionString = connectionString;
            }

            public void Upload(PictureBox pictureBox)
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO ImagesTbl (Images) VALUES (@image)";

                    var image = new Bitmap(pictureBox.Image);
                    using (var memoryStream = new MemoryStream())
                    {
                        image.Save(memoryStream, ImageFormat.Jpeg);
                        memoryStream.Position = 0;

                        var sqlParameter = new SqlParameter("@image", SqlDbType.VarBinary, (int)memoryStream.Length)
                        {
                            Value = memoryStream.ToArray()
                        };
                        command.Parameters.Add(sqlParameter);
                    }
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Image added", "ddipper");
                }
            }
        }

        class ImageRetriever
        {
            private readonly string _connectionString;

            public ImageRetriever(string connectionString)
            {
                _connectionString = connectionString;
            }

            public void Retrieve(PictureBox pictureBox, int id)
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Images FROM ImagesTbl WHERE Id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var imageData = (byte[])reader["Images"];
                            using (var memoryStream = new MemoryStream(imageData))
                            {
                                pictureBox.Image = Image.FromStream(memoryStream);
                            }
                        }
                    }
                }
            }
        }

        private void buttonSetPhoto_Click(object sender, EventArgs e)
        {
            var uploader = new ImageUploader(con);
            uploader.Upload(pictureSet);
        }

        private void buttonVIew_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(textBoxId.Text);
            var retriever = new ImageRetriever(con);
            retriever.Retrieve(pictureView, id);
        }
    }
}
