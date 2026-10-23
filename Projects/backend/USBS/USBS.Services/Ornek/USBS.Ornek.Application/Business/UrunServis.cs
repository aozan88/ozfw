using Ornek.Application.Contract;

namespace Ornek.Application.Business
{
    // Bu dosya generator tarafından bir kez oluşturuldu ve artık size ait; yeniden üretimde değiştirilmez.
    // Ek bağımlılıkları constructor'a ekleyin, iş kurallarını BaseService metotlarını override ederek yazın.
    public partial class UrunServis
    {
        private readonly IUrunRepository _repository;

        public UrunServis(IUrunRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
