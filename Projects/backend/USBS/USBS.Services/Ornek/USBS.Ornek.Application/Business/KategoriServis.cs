using Ornek.Application.Contract;

namespace Ornek.Application.Business
{
    // Bu dosya generator tarafından bir kez oluşturuldu ve artık size ait; yeniden üretimde değiştirilmez.
    // Ek bağımlılıkları constructor'a ekleyin, iş kurallarını BaseService metotlarını override ederek yazın.
    public partial class KategoriServis
    {
        private readonly IKategoriRepository _repository;

        public KategoriServis(IKategoriRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
