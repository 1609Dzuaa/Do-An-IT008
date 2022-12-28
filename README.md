# Phan mem quan ly cua hang xe may
*Recommended Markdown Viewer: [Markdown Editor](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.MarkdownEditor2)*

## Việc cần làm
- Danh sách đã đc sắp xếp theo thứ tự ưu tiên
- Danh sách không bao gồm các việc cần làm ở phần danh mục vì nó cũng khá phức tạp và t sẽ tự quyết định nên thay đổi gì ở đó
- Những việc nằm ngoài danh sách này gần như chỉ là những yếu tố phụ (cộng thêm điểm) và sẽ không nên được ưu tiên vào thời điểm này
- Nếu ai cảm thấy trang của mình ổn rồi thì nên qua giúp những trang khác trước khi quyết định phát triển 1 tính năng thêm

### Thay đổi chung
1. Cho phép người dùng custom ID ở phần cài đặt
2. Đồng bộ lại các connection string theo như chuỗi ở mục `Connection string`
3. Sửa lại toàn bộ đường dẫn file thành đường dẫn tương đối (relative)

### Nhập xuất
1. Ở mục sản phẩm khi click vào 1 sản phẩm sẽ hiện thông tin cơ bản của sp đó, có 2 nút là nhập thêm hàng và tạo hóa đơn
2. Xóa 2 textbox số lượng và giá bán ở mục thêm mới sản phẩm
3. Xử lý kiểm tra điều kiện số lượng ở hóa đơn không được phép lớn hơn số lượng hàng tồn kho, và mỗi lần tạo hóa đơn mới là tự động trừ đi lượng hàng tồn
4. Các textbox nhập ID sửa lại thành combobox, hiển thị thành từng thẻ có vài thông tin quan trọng và cũng cho phép người dùng nhập vào để tìm kiểm (filter)
5. Xử lý lostfocus và keydown ở các textbox phần nhập xuất, đánh dấu * vào những ô bắt buộc phải có dữ liệu (not null)

### Báo cáo
1. Cải thiện thêm phần đồ thị, hiển thị các cột thưa ra, cho phép zoom theo trục hoành

### Dashboard
1. Xem lại phần thay đổi avatar, lên lịch và lịch sử hoạt động

### Về file Resources.resx
- Mn nhớ sử dụng file này cho các tài nguyên liên quan tới giao diện như màu sắc, hình nền, icon... hoặc những chuỗi (string) được sử dụng nhiều nơi. Có thể xem đây là 1 thư viện tổng cho chương trình.

### Connection string
- Sử dụng string dưới đây thay cho 1 connection string thông thường sẽ có những lợi ích sau:
    + Tiết kiệm thời gian code
	+ Code sẽ được đồng bộ, khi test tránh trường hợp connection string khác nhau dẫn đến chỗ có bug chỗ không
	+ Code gọn và dễ nhìn, dễ hiểu hơn
- `System.Configuration.ConfigurationManager.ConnectionStrings["Data"].ConnectionString`

## Yêu cầu chung
- Giải thích rõ ràng đối tượng người dùng mà ứng dụng nhắm đến, lợi ích của ứng dụng, ứng dụng có thân thiện với người dùng không là 1 điểm cộng
- Trang dữ liệu làm dưới dạng lưới, cho người dùng thao tác trên đó cũng là 1 điểm cộng.  
- Khi dữ liệu bị thay đổi(cập nhật, xoá, sửa) thì cũng ảnh hưởng trên Database.
- Database không cần quá chuyên sâu, cầu kì vì hầu như cả lớp đều trên tư tưởng vừa học vừa làm.
- Kiểm tra Bố Cục rành mạch, Đẹp Mắt thì càng tốt, không được đơn giản quá.
- Để ý các trường hợp người dùng nhập liệu sai sót(số xe tồn kho <0, blah blah ...), chú ý dữ liệu đầu vào.
- Nếu được, hãy thêm vài tính năng đặc biệt cho ứng dụng, sẽ có điểm cộng cho phần này.
- Clean Code cũng 1 điểm cộng, các hàm mạch lạc rõ ràng để trong tương lai có thể dễ dàng sửa chữa, bảo trì.
- Áp dụng các kiến thức đã học như OOP, thuật toán đơn giản vào ứng dụng thì càng tốt.
- Các thành viên nhóm ai cũng phải giải thích đc một vài tính năng của ứng dụng.
- Thầy chấm trên tư tưởng thoải mái nhưng ứng dụng thì không đc đơn giản quá.