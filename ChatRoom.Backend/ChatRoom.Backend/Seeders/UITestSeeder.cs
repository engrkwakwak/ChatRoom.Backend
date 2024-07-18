using Dapper;
using System.Data;

namespace ChatRoom.Backend.Seeders
{
    public class UITestSeeder
    {
        private readonly IDbConnection _connection;

        public UITestSeeder(IDbConnection connection)
        {
            _connection = connection;
        }

        public void Seed() 
        {
            _connection.Execute("INSERT INTO [Users]([f_username], [f_display_name], [f_email], [f_password_hash], [f_is_email_verified]) VALUES('the_flash', 'Barry Allen', 'barryallen@test.com', '$2a$11$VPWCJHM/qPtAcLipuJ4k9evJAJdpLDQyso4..iqFSrNHzJ71dYgIa', 1)");
            _connection.Execute("INSERT INTO [Users]([f_username], [f_display_name], [f_email], [f_password_hash], [f_is_email_verified]) VALUES('batman', 'Bruce Waynes', 'brucewaynes@test.com', '$2a$11$VPWCJHM/qPtAcLipuJ4k9evJAJdpLDQyso4..iqFSrNHzJ71dYgIa', 1)");
            _connection.Execute("INSERT INTO [Users]([f_username], [f_display_name], [f_email], [f_password_hash], [f_is_email_verified]) VALUES('captain_america', 'Steve Rogers', 'steverogers@test.com', '$2a$11$VPWCJHM/qPtAcLipuJ4k9evJAJdpLDQyso4..iqFSrNHzJ71dYgIa', 1)");
            _connection.Execute("INSERT INTO [Users]([f_username], [f_display_name], [f_email], [f_password_hash], [f_is_email_verified]) VALUES('iron_man', 'Tony Stark', 'tonystark@test.com', '$2a$11$VPWCJHM/qPtAcLipuJ4k9evJAJdpLDQyso4..iqFSrNHzJ71dYgIa', 1)");
            _connection.Execute("INSERT INTO [Users]([f_username], [f_display_name], [f_email], [f_password_hash], [f_is_email_verified]) VALUES('scarlet_witch', 'Wanda Maximoff', 'wandamaximoff@test.com', '$2a$11$VPWCJHM/qPtAcLipuJ4k9evJAJdpLDQyso4..iqFSrNHzJ71dYgIa', 1)");
            _connection.Execute("INSERT INTO [Users]([f_username], [f_display_name], [f_email], [f_password_hash], [f_is_email_verified]) VALUES('iron_man', 'Tony Stark', 'tonystark@test.com', '$2a$11$VPWCJHM/qPtAcLipuJ4k9evJAJdpLDQyso4..iqFSrNHzJ71dYgIa', 1)");
            _connection.Execute("INSERT INTO [Users]([f_username], [f_display_name], [f_email], [f_password_hash], [f_is_email_verified]) VALUES('spider_man', 'Peter Parker', 'peterparker@test.com', '$2a$11$VPWCJHM/qPtAcLipuJ4k9evJAJdpLDQyso4..iqFSrNHzJ71dYgIa', 1)");
        }
    }
}
