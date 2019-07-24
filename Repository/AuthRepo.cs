using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Api.Repository
{
    public interface IAuthRepo
    {
        Task<Models.Token> GetToken(Models.AccessCred _auth);
        Task<Models.Token> GetRefreshToken(Models.AccessCred _auth);

        Task SignOut(string refToken, int idUser);
    }



    public class AuthRepo : IAuthRepo
    {
        public System.Data.IDbConnection Db { get; set; }

        public AuthRepo(DAL.IConnection _conn)
        {
            Db = _conn.Db;
        }


        public async Task<Models.Token> GetToken(Models.AccessCred _auth)
        {

            Models.User user = await ValidateUser(_auth.Username, _auth.Password);

            // Controllo login
            if (user != null)
            {
                await UpdateDateLogin(user.UserId);

                return await CreateToken(user.UserId, user.RoleId, "");
            }

            return null;

        }

        public async Task<Models.Token> GetRefreshToken(Models.AccessCred cred)
        {
            // Ricavo il refresh token
            Models.RefreshToken refT = await GetRefToken(cred.Refresh_token);

            // Controllo refresh token
            if (refT != null)
            {
                // Controllo se il refresh token è scaduto
                if (DateTime.Compare(refT.DateExpires, DateTime.Now) > 0)
                {
                    // Ricavo i dati utente
                    Models.User user = GetUser(refT.UserId);

                    return await CreateToken(user.UserId, user.RoleId, cred.Refresh_token);
                }
                else
                {
                    // Elimino il refresh token scaduto
                    await DeleteRefreshToken(cred.Refresh_token);
                }

            }

            return null;

        }



        public async Task SignOut(string refToken, int idUser)
        {
            await DeleteRefreshToken(refToken);

            await UpdateDateLogout(idUser);
        }


        private async Task UpdateDateLogin(int idUser)
        {
            await Db.ExecuteAsync("UPDATE Web_Users SET DateLogin=GETDATE() WHERE  UserID=@UserID ", new { UserID = idUser });
        }


        private async Task UpdateDateLogout(int idUser)
        {
            await Db.ExecuteAsync("UPDATE Web_Users SET DateLogout=GETDATE() WHERE  UserID=@UserID ", new { UserID = idUser });
        }



        private async Task<Models.Token> CreateToken(int userID, int roleId, string refTokenOld)
        {

            Models.Token token = null;

            // Creo il token
            Services.JwtHandler jwt = new Services.JwtHandler();
            {
                token = jwt.CreateToken(userID, roleId);
            }

            // Pulizia token scaduti
            await ClearTokenExpired(userID);

            // Controllo se eliminare il token precedente
            if (refTokenOld.Length > 0)
            {
                await DeleteRefreshToken(refTokenOld);
            }


            // Memorizzo il refreshToken
            Models.RefreshToken refT = new Models.RefreshToken
            {
                DateIssued = DateTime.Now,
                DateExpires = DateTime.Now.AddDays(30),  // 1 Mese
                Refresh_token = token.Refresh_token,
                UserId = userID
            };

            await SaveRefreshToken(refT);
            // -----------------


            return token;
        }


        private async Task<Models.User> ValidateUser(string username, string pwd)
        {

            var param = new DynamicParameters();
            param.Add("@UserName", username);
            param.Add("@PasswordHash", pwd);

            return await Db.QuerySingleOrDefaultAsync<Models.User>("SELECT * FROM Web_Users WHERE UserName=@UserName AND PasswordHash=HASHBYTES('SHA2_512', @PasswordHash) AND Enabled=1", param);
        }


        private Models.User GetUser(int idUser)
        {
            return Db.Query<Models.User>("SELECT * FROM Web_Users WHERE UserID=@UserID ", new { UserID = idUser }).SingleOrDefault();
        }

        private async Task SaveRefreshToken(Models.RefreshToken refT)
        {
            await Db.ExecuteAsync("INSERT INTO Web_RefreshToken(UserId, Refresh_Token, DateIssued, DateExpires) VALUES(@UserId, @Refresh_Token, @DateIssued, @DateExpires)", refT);
        }

        private async Task DeleteRefreshToken(string refTokenOld)
        {
            await Db.ExecuteAsync("DELETE FROM Web_RefreshToken WHERE Refresh_Token=@Refresh_Token", new { Refresh_Token = refTokenOld });
        }

        private async Task<Models.RefreshToken> GetRefToken(string refToken)
        {
            return await Db.QueryFirstOrDefaultAsync<Models.RefreshToken>("SELECT * FROM Web_RefreshToken WHERE Refresh_Token=@Refresh_Token", new { Refresh_Token = refToken });

        }

        private async Task ClearTokenExpired(int userId)
        {
            await Db.ExecuteAsync("DELETE FROM Web_RefreshToken WHERE UserId=@UserId AND DateExpires< GETDATE()", new { UserId = userId });
        }
    }


}
