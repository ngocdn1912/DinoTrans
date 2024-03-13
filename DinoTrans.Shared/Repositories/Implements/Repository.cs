using DinoTrans.Shared.Data;
using DinoTrans.Shared.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Repositories.Implements
{
    public class Repository<T> : IRepository<T> where T : class
    {
        // Trường lưu trữ đối tượng quản lý kết nối và tương tác với cơ sở dữ liệu
        protected DinoTransDbContext _context;

        // Trường lưu trữ đối tượng DbSet<T> để thực hiện các thao tác trên đối tượng T
        private DbSet<T> _dbSet;

        // Thuộc tính để truy cập _dbSet, kiểm tra và khởi tạo nếu cần thiết
        protected DbSet<T> DbSet
        {
            get
            {
                if (_dbSet == null)
                {
                    _dbSet = _context.Set<T>();
                }
                return _dbSet;
            }
        }

        // Constructor, nhận một đối tượng DinoTransDbContext để thiết lập kết nối
        public Repository(DinoTransDbContext context)
        {
            _context = context;
        }

        // Các phương thức triển khai từ interface IRepository<T>

        // Truy vấn dữ liệu dạng IQueryable
        public IQueryable<T> Queryable()
        {
            return DbSet.AsQueryable();
        }

        // Truy vấn dữ liệu dạng IQueryable không theo dõi thay đổi
        public IQueryable<T> AsNoTracking()
        {
            return DbSet.AsNoTracking();
        }

        // Lấy đối tượng theo khóa chính
        public T Get(object id)
        {
            return DbSet.Find(id);
        }

        // Lấy đối tượng theo khóa chính (bất đồng bộ)
        public async Task<T> GetAsync(object id)
        {
            return await DbSet.FindAsync(id);
        }

        // Thêm mới đối tượng vào cơ sở dữ liệu
        public T Add(T entity)
        {
            DbSet.Add(entity);
            return entity;
        }

        // Thêm mới một danh sách đối tượng vào cơ sở dữ liệu
        public IList<T> AddRange(IList<T> entities)
        {
            DbSet.AddRange(entities);
            return entities;
        }

        // Cập nhật đối tượng trong cơ sở dữ liệu
        public T Update(T entity)
        {
            DbSet.Update(entity);
            return entity;
        }

        // Cập nhật một danh sách đối tượng trong cơ sở dữ liệu
        public IList<T> UpdateRange(IList<T> entities)
        {
            DbSet.UpdateRange(entities);
            return entities;
        }

        // Xóa đối tượng khỏi cơ sở dữ liệu
        public bool Delete(T entity)
        {
            DbSet.Remove(entity);
            return true;
        }

        // Xóa một danh sách đối tượng khỏi cơ sở dữ liệu
        public bool DeleteRange(IList<T> entities)
        {
            DbSet.RemoveRange(entities);
            return true;
        }

        // Lưu các thay đổi vào cơ sở dữ liệu và trả về số lượng bản ghi bị ảnh hưởng
        public int SaveChange()
        {
            return _context.SaveChanges();
        }

        // Lưu các thay đổi vào cơ sở dữ liệu (bất đồng bộ) và trả về số lượng bản ghi bị ảnh hưởng
        public async Task<int> SaveChangeAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
