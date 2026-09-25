import os
from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.enum.shapes import MSO_SHAPE

def build_presentation():
    prs = Presentation()
    prs.slide_width = Inches(13.333)
    prs.slide_height = Inches(7.5)
    blank_layout = prs.slide_layouts[6]

    # Theme Palette - Modern Dojo / Ninja Wood & Gold
    BG_DARK = RGBColor(20, 13, 9)             # Deep Dojo Charcoal Wood
    BG_CARD = RGBColor(34, 23, 16)            # Rich Wood Card
    BG_CARD_LIGHT = RGBColor(46, 32, 22)      # Highlighted Card Header
    BORDER_GOLD = RGBColor(212, 160, 60)      # Ninja Gold Border
    BORDER_MUTED = RGBColor(90, 65, 45)       # Subtle Wood Border

    TEXT_TITLE = RGBColor(255, 250, 240)      # Crisp White
    TEXT_GOLD = RGBColor(250, 195, 75)        # Bright Ninja Gold
    TEXT_BODY = RGBColor(235, 228, 218)       # Off-white / Cream
    TEXT_MUTED = RGBColor(180, 165, 150)      # Muted Tan

    ACCENT_RED = RGBColor(235, 59, 45)        # Bomb Crimson
    ACCENT_GREEN = RGBColor(46, 204, 113)     # Kiwi Green
    ACCENT_ORANGE = RGBColor(243, 156, 18)    # Tangerine Orange
    ACCENT_CYAN = RGBColor(52, 152, 219)      # Sky Blue

    # Sprite Paths
    BASE_DIR = os.path.dirname(os.path.abspath(__file__))
    SPRITE_DIR = os.path.join(BASE_DIR, "Assets", "Sprites")

    def sprite(name):
        p = os.path.join(SPRITE_DIR, name)
        return p if os.path.exists(p) else None

    watermelon = sprite("Dưa hấu.png")
    apple = sprite("táo.png")
    orange = sprite("Cam.png")
    banana = sprite("Chuoi.png")
    strawberry = sprite("DauTay.png")
    bomb = sprite("bomb.png")
    bg_wood = sprite("Backgrounds.png")

    def set_slide_background(slide):
        # Base background fill
        bg = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
        bg.fill.solid()
        bg.fill.fore_color.rgb = BG_DARK
        bg.line.fill.background()

        # Top Gold Accent Line
        gold_line = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(0.08))
        gold_line.fill.solid()
        gold_line.fill.fore_color.rgb = TEXT_GOLD
        gold_line.line.fill.background()

        # Bottom Dojo Footer Bar
        ft = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(7.15), Inches(13.333), Inches(0.35))
        ft.fill.solid()
        ft.fill.fore_color.rgb = RGBColor(14, 9, 6)
        ft.line.fill.background()

        tb_ft = slide.shapes.add_textbox(Inches(0.8), Inches(7.18), Inches(11.733), Inches(0.3))
        p = tb_ft.text_frame.paragraphs[0]
        p.text = "FRUIT NINJA 2D  •  ĐỒ ÁN PHÁT TRIỂN GAME 2D  •  KHOA CÔNG NGHỆ THÔNG TIN"
        p.font.size = Pt(9.5)
        p.font.color.rgb = RGBColor(140, 120, 100)
        p.font.name = "Segoe UI"

    def add_header(slide, tag_text, title_text, subtitle_text=""):
        # Tag Badge
        tag_box = slide.shapes.add_textbox(Inches(0.8), Inches(0.38), Inches(9), Inches(0.3))
        tf_tag = tag_box.text_frame
        tf_tag.word_wrap = True
        tf_tag.margin_left = tf_tag.margin_right = tf_tag.margin_top = tf_tag.margin_bottom = 0
        p_tag = tf_tag.paragraphs[0]
        p_tag.text = f"★  {tag_text.upper()}  ★"
        p_tag.font.size = Pt(10.5)
        p_tag.font.bold = True
        p_tag.font.color.rgb = TEXT_GOLD
        p_tag.font.name = "Segoe UI"

        # Main Title
        t_box = slide.shapes.add_textbox(Inches(0.8), Inches(0.68), Inches(10), Inches(0.6))
        tf_t = t_box.text_frame
        tf_t.word_wrap = True
        tf_t.margin_left = tf_t.margin_right = tf_t.margin_top = tf_t.margin_bottom = 0
        p_t = tf_t.paragraphs[0]
        p_t.text = title_text
        p_t.font.size = Pt(24)
        p_t.font.bold = True
        p_t.font.color.rgb = TEXT_TITLE
        p_t.font.name = "Segoe UI"

        # Subtitle
        if subtitle_text:
            sub_box = slide.shapes.add_textbox(Inches(0.8), Inches(1.3), Inches(10), Inches(0.35))
            tf_sub = sub_box.text_frame
            tf_sub.word_wrap = True
            tf_sub.margin_left = tf_sub.margin_right = tf_sub.margin_top = tf_sub.margin_bottom = 0
            p_sub = tf_sub.paragraphs[0]
            p_sub.text = subtitle_text
            p_sub.font.size = Pt(12)
            p_sub.font.color.rgb = TEXT_MUTED
            p_sub.font.name = "Segoe UI"

    def create_card(slide, left, top, width, height, border_color=BORDER_MUTED, bg_color=BG_CARD):
        card = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, height)
        card.fill.solid()
        card.fill.fore_color.rgb = bg_color
        card.line.color.rgb = border_color
        card.line.width = Pt(1.2)
        return card

    # ==========================================
    # SLIDE 1: TIÊU ĐỀ (TITLE SLIDE)
    # ==========================================
    s1 = prs.slides.add_slide(blank_layout)
    set_slide_background(s1)

    # Center Hero Frame
    create_card(s1, Inches(0.8), Inches(0.8), Inches(11.733), Inches(5.95), BORDER_GOLD, BG_CARD)

    # Top Pill Tag
    pill = s1.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(1.3), Inches(1.2), Inches(5.2), Inches(0.42))
    pill.fill.solid()
    pill.fill.fore_color.rgb = BG_CARD_LIGHT
    pill.line.color.rgb = BORDER_GOLD
    pill.line.width = Pt(1)
    p_pill = pill.text_frame.paragraphs[0]
    p_pill.alignment = PP_ALIGN.CENTER
    p_pill.text = "⚔️  ĐỒ ÁN PHÁT TRIỂN GAME 2D  •  CNTT  ⚔️"
    p_pill.font.size = Pt(11)
    p_pill.font.bold = True
    p_pill.font.color.rgb = TEXT_GOLD
    p_pill.font.name = "Segoe UI"

    # Main Title
    tb_title = s1.shapes.add_textbox(Inches(1.3), Inches(1.8), Inches(7.5), Inches(1.2))
    p_m1 = tb_title.text_frame.paragraphs[0]
    p_m1.text = "FRUIT NINJA 2D"
    p_m1.font.size = Pt(44)
    p_m1.font.bold = True
    p_m1.font.color.rgb = TEXT_GOLD
    p_m1.font.name = "Segoe UI"

    p_m2 = tb_title.text_frame.add_paragraph()
    p_m2.text = "GAME CHÉM HOA QUẢ PHONG CÁCH VÕ QUÁN"
    p_m2.font.size = Pt(18)
    p_m2.font.bold = True
    p_m2.font.color.rgb = TEXT_TITLE
    p_m2.font.name = "Segoe UI"

    # Divider bar
    div = s1.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(1.3), Inches(3.25), Inches(6.8), Inches(0.04))
    div.fill.solid()
    div.fill.fore_color.rgb = ACCENT_ORANGE
    div.line.fill.background()

    # Project Info Box
    info_box = create_card(s1, Inches(1.3), Inches(3.55), Inches(6.8), Inches(2.7), BORDER_MUTED, BG_CARD_LIGHT)
    tb_info = s1.shapes.add_textbox(Inches(1.55), Inches(3.7), Inches(6.3), Inches(2.4))
    tf_info = tb_info.text_frame
    tf_info.word_wrap = True

    meta_items = [
        ("Giảng viên hướng dẫn:", "ThS. [Họ và Tên Giảng Viên]"),
        ("Sinh viên thực hiện:", "[Họ và Tên Sinh Viên]"),
        ("Mã số sinh viên (MSSV):", "[Mã số sinh viên]"),
        ("Chuyên ngành:", "Kỹ thuật Phần mềm / Công nghệ Thông tin"),
        ("Nền tảng & Công nghệ:", "Unity Engine (URP 2D)  •  C# Scripting")
    ]
    for i, (k, v) in enumerate(meta_items):
        p = tf_info.paragraphs[0] if i == 0 else tf_info.add_paragraph()
        r1 = p.add_run()
        r1.text = f"• {k} "
        r1.font.bold = True
        r1.font.size = Pt(12)
        r1.font.color.rgb = TEXT_GOLD
        r1.font.name = "Segoe UI"

        r2 = p.add_run()
        r2.text = v
        r2.font.size = Pt(12)
        r2.font.color.rgb = TEXT_BODY
        r2.font.name = "Segoe UI"
        p.space_after = Pt(5)

    # Fruit artwork on right
    if watermelon:
        s1.shapes.add_picture(watermelon, Inches(8.7), Inches(1.3), width=Inches(2.7))
    if bomb:
        s1.shapes.add_picture(bomb, Inches(10.2), Inches(3.8), width=Inches(1.8))
    if orange:
        s1.shapes.add_picture(orange, Inches(8.3), Inches(4.5), width=Inches(1.7))

    # ==========================================
    # SLIDE 2: GIỚI THIỆU ĐỀ TÀI & MỤC TIÊU
    # ==========================================
    s2 = prs.slides.add_slide(blank_layout)
    set_slide_background(s2)
    add_header(s2, "Tổng quan đề tài", "GIỚI THIỆU ĐỀ TÀI & MỤC TIÊU PHÁT TRIỂN", "Định hướng phát triển tựa game 2D mang tính giải trí cao, kỹ thuật tối ưu và lối chơi mượt mà")

    pillars = [
        ("01", "Ý TƯỞNG & ĐỀ TÀI", TEXT_GOLD, apple, [
            ("Lối chơi kinh điển:", "Kế thừa phong cách Fruit Ninja huyền thoại, dễ chơi, kích thích phản xạ."),
            ("Chủ đề Võ quán:", "Không gian kiếm đạo mộc mạc kết hợp hoạt họa hoa quả tươi sáng."),
            ("Tính giải trí tức thì:", "Phù hợp mọi lứa tuổi, giải tỏa căng thẳng với các vệt chém sảng khoái.")
        ]),
        ("02", "MỤC TIÊU KỸ THUẬT", ACCENT_ORANGE, banana, [
            ("Làm chủ Unity 2D:", "Vận dụng Rigidbody2D, CircleCollider2D và tia Raycasting."),
            ("Tối ưu hóa bộ nhớ:", "Áp dụng Object Pooling triệt tiêu giật lag do Garbage Collection."),
            ("Cấu trúc Clean Code:", "Mô hình Singleton, Event-Driven và phân tách Manager mạch lạc.")
        ]),
        ("03", "MỤC TIÊU SẢN PHẨM", ACCENT_GREEN, strawberry, [
            ("Chuẩn 60 FPS mượt mà:", "Vận hành ổn định trên nhiều cấu hình thiết bị khác nhau."),
            ("Trải nghiệm xúc giác:", "Hiệu ứng vệt kiếm phát sáng, rung chấn camera, âm thanh sống động."),
            ("Dễ dàng mở rộng:", "Khung sườn kiến trúc sẵn sàng bổ sung skin kiếm và chế độ chơi mới.")
        ]),
    ]

    card_w2 = Inches(3.64)
    card_h2 = Inches(5.1)
    for idx, (num, p_title, p_col, p_img, pts) in enumerate(pillars):
        x = Inches(0.8 + idx * 4.04)
        create_card(s2, x, Inches(1.8), card_w2, card_h2, p_col, BG_CARD)

        # Header bar
        hb = s2.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, x + Inches(0.15), Inches(1.95), card_w2 - Inches(0.3), Inches(0.8))
        hb.fill.solid()
        hb.fill.fore_color.rgb = BG_CARD_LIGHT
        hb.line.color.rgb = p_col
        hb.line.width = Pt(1)

        tb_h = s2.shapes.add_textbox(x + Inches(0.2), Inches(2.0), card_w2 - Inches(0.4), Inches(0.7))
        p = tb_h.text_frame.paragraphs[0]
        p.text = f"MỤC TIÊU {num} • {p_title}"
        p.font.bold = True
        p.font.size = Pt(12)
        p.font.color.rgb = p_col
        p.font.name = "Segoe UI"

        # Content bullets
        tb_b = s2.shapes.add_textbox(x + Inches(0.2), Inches(2.9), card_w2 - Inches(0.4), Inches(3.0))
        tf_b = tb_b.text_frame
        tf_b.word_wrap = True
        for p_i, (bold_txt, norm_txt) in enumerate(pts):
            p = tf_b.paragraphs[0] if p_i == 0 else tf_b.add_paragraph()
            r1 = p.add_run()
            r1.text = f"✔ {bold_txt} "
            r1.font.bold = True
            r1.font.size = Pt(11.5)
            r1.font.color.rgb = TEXT_TITLE
            r1.font.name = "Segoe UI"

            r2 = p.add_run()
            r2.text = norm_txt
            r2.font.size = Pt(11)
            r2.font.color.rgb = TEXT_MUTED
            r2.font.name = "Segoe UI"
            p.space_after = Pt(10)

        # Fruit badge on bottom
        if p_img:
            s2.shapes.add_picture(p_img, x + Inches(2.4), Inches(5.75), width=Inches(1.0))

    # ==========================================
    # SLIDE 3: TỔNG QUAN GAMEPLAY & LUẬT CHƠI
    # ==========================================
    s3 = prs.slides.add_slide(blank_layout)
    set_slide_background(s3)
    add_header(s3, "Cơ chế trò chơi", "TỔNG QUAN GAMEPLAY & LUẬT CHƠI", "Lối chơi trực quan, phản xạ nhanh, kích thích sự tập trung và độ chính xác")

    gameplay_grid = [
        ("⚔️  VỆT KIẾM CHÉM (BLADE SLICE)", TEXT_GOLD, [
            ("Thao tác trực quan:", "Nhấn giữ chuột hoặc vuốt chạm màn hình cảm ứng để vung kiếm."),
            ("Ngưỡng tốc độ (Min Velocity):", "Đường chém phải đạt tốc độ tối thiểu mới kích hoạt nhát cắt."),
            ("TrailRenderer động:", "Vệt kiếm sáng trắng bám đuổi theo thao tác mượt mà không trễ.")
        ]),
        ("🍉  HOA QUẢ VẬT LÝ TỰ NHIÊN", ACCENT_GREEN, [
            ("Quỹ đạo Parabol:", "Quả bắn từ mép dưới với lực đẩy và góc xiên ngẫu nhiên."),
            ("Đa dạng chủng loại:", "Dưa hấu, Táo, Cam, Chuối, Dâu tây với kích cỡ và điểm số riêng."),
            ("Tách đôi sống động:", "Bị chém trúng tách thành 2 nửa FruitHalf xoay rơi tự do.")
        ]),
        ("💣  CHƯỚNG NGẠI BẪY BOM", ACCENT_RED, [
            ("Yếu tố bất ngờ:", "Bom đen bay xen kẽ giữa các đợt bắn hoa quả để thử thách mắt nhìn."),
            ("Trừng phạt nghiêm khắc:", "Chém trúng bom lập tức kích nổ, rung màn hình và mất 1 Tim."),
            ("Chiến thuật dừng kiếm:", "Yêu cầu người chơi kiểm soát đường vung kiếm chuẩn xác.")
        ]),
        ("❤️  SINH TỒN 3 MẠNG & ĐỘ KHÓ", ACCENT_ORANGE, [
            ("3 Tim sinh mệnh:", "Bắt đầu với 3 mạng; khi hết mạng trò chơi kết thúc (Game Over)."),
            ("Chế độ chơi xả stress:", "Quả rơi xuống màn hình không trừ mạng, tập trung tạo Combo."),
            ("Độ khó tăng dần:", "Mỗi 10s tần suất và số lượng quả tăng lên, đẩy cao trào trận đấu.")
        ])
    ]

    for idx, (title, col, pts) in enumerate(gameplay_grid):
        c = idx % 2
        r = idx // 2
        x = Inches(0.8 + c * 5.95)
        y = Inches(1.8 + r * 2.65)
        w = Inches(5.78)
        h = Inches(2.45)
        create_card(s3, x, y, w, h, col, BG_CARD)

        tb_h = s3.shapes.add_textbox(x + Inches(0.2), y + Inches(0.12), w - Inches(0.4), Inches(0.4))
        p = tb_h.text_frame.paragraphs[0]
        p.text = title
        p.font.bold = True
        p.font.size = Pt(13)
        p.font.color.rgb = col
        p.font.name = "Segoe UI"

        tb_b = s3.shapes.add_textbox(x + Inches(0.2), y + Inches(0.55), w - Inches(0.4), Inches(1.8))
        tf_b = tb_b.text_frame
        tf_b.word_wrap = True
        for p_i, (bold_txt, norm_txt) in enumerate(pts):
            p = tf_b.paragraphs[0] if p_i == 0 else tf_b.add_paragraph()
            r1 = p.add_run()
            r1.text = f"• {bold_txt} "
            r1.font.bold = True
            r1.font.size = Pt(11)
            r1.font.color.rgb = TEXT_TITLE
            r1.font.name = "Segoe UI"

            r2 = p.add_run()
            r2.text = norm_txt
            r2.font.size = Pt(10.5)
            r2.font.color.rgb = TEXT_MUTED
            r2.font.name = "Segoe UI"
            p.space_after = Pt(3)

    # ==========================================
    # SLIDE 4: CÔNG NGHỆ & CÔNG CỤ PHÁT TRIỂN
    # ==========================================
    s4 = prs.slides.add_slide(blank_layout)
    set_slide_background(s4)
    add_header(s4, "Công nghệ ứng dụng", "NGĂN XẾP CÔNG NGHỆ & CÔNG CỤ PHÁT TRIỂN", "Bộ công cụ chuẩn công nghiệp game 2D, đảm bảo tính ổn định và khả năng tối ưu cao")

    tech_cols = [
        ("UNITY ENGINE", "2022 / 6 LTS (URP 2D)", TEXT_GOLD, [
            "Công cụ phát triển game 2D/3D hàng đầu.",
            "Universal Render Pipeline tối ưu hóa hiển thị.",
            "Xuất đa nền tảng: PC, WebGL, Mobile."
        ]),
        ("NGÔN NGỮ C#", "Object-Oriented Programming", ACCENT_ORANGE, [
            "Hiện thực toàn bộ Gameplay logic & Physics.",
            "Sử dụng Generic Collections tối ưu truy xuất.",
            "Tuân thủ nghiêm ngặt nguyên lý Clean Code."
        ]),
        ("VẬT LÝ UNITY 2D", "Physics2D & Collision", ACCENT_GREEN, [
            "Rigidbody2D mô phỏng trọng lực Parabol tự nhiên.",
            "Raycast2D cắt kiếm chính xác không độ trễ.",
            "LayerMask phân tầng va chạm Fruit / Bomb riêng."
        ]),
        ("GRAPHICS & VFX", "Particle & Post-Processing", ACCENT_RED, [
            "TrailRenderer tạo vệt sáng kiếm mượt mà.",
            "Particle System tạo hiệu ứng nước ép tóe ra.",
            "Camera Shake tạo rung chấn mạnh khi nổ bom."
        ])
    ]

    card_w4 = Inches(2.78)
    card_h4 = Inches(5.1)
    for idx, (t_title, t_sub, t_col, bullets) in enumerate(tech_cols):
        x = Inches(0.8 + idx * 2.99)
        create_card(s4, x, Inches(1.8), card_w4, card_h4, t_col, BG_CARD)

        # Header bar
        hb = s4.shapes.add_shape(MSO_SHAPE.RECTANGLE, x + Inches(0.12), Inches(1.95), card_w4 - Inches(0.24), Inches(0.9))
        hb.fill.solid()
        hb.fill.fore_color.rgb = BG_CARD_LIGHT
        hb.line.color.rgb = t_col
        hb.line.width = Pt(1)

        tb_h = s4.shapes.add_textbox(x + Inches(0.15), Inches(2.0), card_w4 - Inches(0.3), Inches(0.85))
        p1 = tb_h.text_frame.paragraphs[0]
        p1.text = t_title
        p1.font.bold = True
        p1.font.size = Pt(13)
        p1.font.color.rgb = t_col
        p1.font.name = "Segoe UI"

        p2 = tb_h.text_frame.add_paragraph()
        p2.text = t_sub
        p2.font.size = Pt(10)
        p2.font.color.rgb = TEXT_MUTED
        p2.font.name = "Segoe UI"

        # Content
        tb_c = s4.shapes.add_textbox(x + Inches(0.15), Inches(3.05), card_w4 - Inches(0.3), Inches(3.6))
        tf_c = tb_c.text_frame
        tf_c.word_wrap = True
        for b_i, item in enumerate(bullets):
            p = tf_c.paragraphs[0] if b_i == 0 else tf_c.add_paragraph()
            p.text = f"❖  {item}"
            p.font.size = Pt(11)
            p.font.color.rgb = TEXT_TITLE
            p.font.name = "Segoe UI"
            p.space_after = Pt(12)

    # ==========================================
    # SLIDE 5: KIẾN TRÚC HỆ THỐNG & CÁC MODULE
    # ==========================================
    s5 = prs.slides.add_slide(blank_layout)
    set_slide_background(s5)
    add_header(s5, "Thiết kế phần mềm", "KIẾN TRÚC HỆ THỐNG & CÁC MODULE CHÍNH", "Cấu trúc Component-Based kết hợp Singleton Pattern đảm bảo code gọn gàng, độc lập")

    # Left: Architecture Diagram Box
    create_card(s5, Inches(0.8), Inches(1.8), Inches(4.7), Inches(5.1), BORDER_GOLD, BG_CARD)
    tb_mtitle = s5.shapes.add_textbox(Inches(1.0), Inches(1.95), Inches(4.3), Inches(0.4))
    p = tb_mtitle.text_frame.paragraphs[0]
    p.text = "🏛️ HỆ THỐNG CÁC MODULE CHÍNH"
    p.font.bold = True
    p.font.size = Pt(13)
    p.font.color.rgb = TEXT_GOLD

    modules = [
        ("GameManager", "Điều phối Game State, Mạng sống, Reset game", TEXT_GOLD),
        ("ScoreManager", "Lưu trữ Điểm số, Combo Streak, High Score", ACCENT_GREEN),
        ("UIManager", "Cập nhật HUD, Màn hình Menu, Game Over", ACCENT_ORANGE),
        ("FruitSpawner", "Tính toán nhịp bắn, góc xiên & lực đẩy Parabol", TEXT_TITLE),
        ("BladeController", "Bắt sự kiện vuốt chuột, tính vận tốc & Raycast", ACCENT_CYAN),
        ("ObjectPool", "Kho tái sử dụng đối tượng Fruit, Half & Bomb", ACCENT_RED)
    ]
    for idx, (m_name, m_desc, m_col) in enumerate(modules):
        my = Inches(2.45 + idx * 0.72)
        mb = s5.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(1.0), my, Inches(4.3), Inches(0.62))
        mb.fill.solid()
        mb.fill.fore_color.rgb = BG_CARD_LIGHT
        mb.line.color.rgb = m_col
        mb.line.width = Pt(1)

        tb_m = s5.shapes.add_textbox(Inches(1.15), my + Inches(0.04), Inches(4.0), Inches(0.55))
        p1 = tb_m.text_frame.paragraphs[0]
        p1.text = f"★ {m_name}"
        p1.font.bold = True
        p1.font.size = Pt(11.5)
        p1.font.color.rgb = m_col

        p2 = tb_m.text_frame.add_paragraph()
        p2.text = m_desc
        p2.font.size = Pt(9.5)
        p2.font.color.rgb = TEXT_MUTED

    # Right: 3 Design Principles
    principles = [
        ("MẪU THIẾT KẾ SINGLETON (SINGLETON PATTERN)", TEXT_GOLD, [
            ("Một thực thể duy nhất:", "GameManager, ScoreManager tồn tại 1 instance duy nhất."),
            ("Truy cập thuận tiện:", "Gọi trực tiếp `GameManager.Instance` không cần tham chiếu Inspector."),
            ("Bảo toàn dữ liệu:", "Dữ liệu điểm số và kỷ lục không bị mất giữa các lần chơi lại.")
        ]),
        ("GIAO TIẾP EVENT-DRIVEN & LOOSE COUPLING", ACCENT_ORANGE, [
            ("Tách biệt logic:", "Logic tính điểm hoàn toàn độc lập với việc vẽ text giao diện UI."),
            ("Phản xạ thời gian thực:", "Khi chém quả, Fruit kích hoạt event để ScoreManager và UIManager đồng thời cập nhật.")
        ]),
        ("QUẢN LÝ VÒNG ĐỜI TRÒ CHƠI (GAME LOOP)", ACCENT_GREEN, [
            ("Game State rõ ràng:", "Quản lý các trạng thái: Menu -> Playing -> Paused -> GameOver."),
            ("Kiểm soát TimeScale:", "Dừng game tức thời `Time.timeScale = 0` khi Game Over mà không gây lỗi luồng vật lý.")
        ])
    ]

    for idx, (p_head, p_col, p_pts) in enumerate(principles):
        py = Inches(1.8 + idx * 1.73)
        create_card(s5, Inches(5.75), py, Inches(6.78), Inches(1.6), p_col, BG_CARD)

        tb_ph = s5.shapes.add_textbox(Inches(5.95), py + Inches(0.08), Inches(6.4), Inches(0.35))
        p = tb_ph.text_frame.paragraphs[0]
        p.text = p_head
        p.font.bold = True
        p.font.size = Pt(12)
        p.font.color.rgb = p_col

        tb_pb = s5.shapes.add_textbox(Inches(5.95), py + Inches(0.38), Inches(6.4), Inches(1.15))
        tf_pb = tb_pb.text_frame
        tf_pb.word_wrap = True
        for pt_i, (b_txt, n_txt) in enumerate(p_pts):
            p = tf_pb.paragraphs[0] if pt_i == 0 else tf_pb.add_paragraph()
            r1 = p.add_run()
            r1.text = f"✔ {b_txt} "
            r1.font.bold = True
            r1.font.size = Pt(10.5)
            r1.font.color.rgb = TEXT_TITLE

            r2 = p.add_run()
            r2.text = n_txt
            r2.font.size = Pt(10.5)
            r2.font.color.rgb = TEXT_MUTED
            p.space_after = Pt(2)

    # ==========================================
    # SLIDE 6: KỸ THUẬT VẾT CHÉM & CẮT ĐÔI HOA QUẢ
    # ==========================================
    s6 = prs.slides.add_slide(blank_layout)
    set_slide_background(s6)
    add_header(s6, "Thuật toán xử lý", "KỸ THUẬT VẾT CHÉM & PHÂN TÁCH TRÁI CÂY", "Kết hợp mượt mà giữa tính toán hình học Vector2, Raycast 2D và hiệu ứng vật lý Rigidbody")

    slicing_steps = [
        ("BƯỚC 01", "NHẬN DIỆN VẾT CHÉM", TEXT_GOLD, [
            ("Lưu vị trí frame:", "Liên tục cập nhật tọa độ chuột `previousPos` và `currentPos`."),
            ("Ngưỡng Min Velocity:", "Chỉ kích hoạt khi tốc độ quẹt đạt ngưỡng cho phép."),
            ("Physics2D.Linecast:", "Bắn tia giữa 2 điểm chuột quét trúng collider quả."),
            ("Bộ lọc HashSet:", "Đảm bảo mỗi nhát quẹt chỉ tính trúng 1 lần trên mỗi quả.")
        ]),
        ("BƯỚC 02", "PHÂN TÁCH FRUITHALF", ACCENT_ORANGE, [
            ("Ẩn quả nguyên lập tức:", "Quả nguyên lập tức ẩn đi (`SetActive: false`) và về Pool."),
            ("Kích hoạt 2 nửa quả:", "Lấy 2 nửa FruitHalf tương ứng từ Object Pool."),
            ("Lực đẩy trái chiều:", "Gán vận tốc nảy: Nửa trái bay sang -X, nửa phải sang +X."),
            ("AddTorque xoay tròn:", "Tạo momen xoắn ngẫu nhiên cho 2 nửa quả xoay rơi.")
        ]),
        ("BƯỚC 03", "HIỆU ỨNG THỊ GIÁC", ACCENT_GREEN, [
            ("TrailRenderer phát sáng:", "Vệt kiếm sáng trắng co đuôi dần trong 0.15 giây."),
            ("Splash Juice Particles:", "Tung hạt nước ép màu sắc đặc trưng tại điểm chém."),
            ("Camera Shake:", "Rung màn hình 0.2s khi chém trúng bom hoặc combo lớn."),
            ("Âm thanh chém ngọt:", "Phát Sound Effect 'xoẹt' tức thì ngay khi va chạm.")
        ])
    ]

    for idx, (step_badge, step_title, step_col, step_pts) in enumerate(slicing_steps):
        x = Inches(0.8 + idx * 4.04)
        create_card(s6, x, Inches(1.8), Inches(3.64), Inches(5.1), step_col, BG_CARD)

        # Step header badge
        hb = s6.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, x + Inches(0.15), Inches(1.95), Inches(3.34), Inches(0.8))
        hb.fill.solid()
        hb.fill.fore_color.rgb = BG_CARD_LIGHT
        hb.line.color.rgb = step_col
        hb.line.width = Pt(1)

        tb_h = s6.shapes.add_textbox(x + Inches(0.2), Inches(2.0), Inches(3.24), Inches(0.7))
        p = tb_h.text_frame.paragraphs[0]
        p.text = f"{step_badge} • {step_title}"
        p.font.bold = True
        p.font.size = Pt(11.5)
        p.font.color.rgb = step_col

        tb_b = s6.shapes.add_textbox(x + Inches(0.18), Inches(2.9), Inches(3.28), Inches(3.8))
        tf_b = tb_b.text_frame
        tf_b.word_wrap = True
        for pt_i, (b_txt, n_txt) in enumerate(step_pts):
            p = tf_b.paragraphs[0] if pt_i == 0 else tf_b.add_paragraph()
            r1 = p.add_run()
            r1.text = f"❖ {b_txt} "
            r1.font.bold = True
            r1.font.size = Pt(11)
            r1.font.color.rgb = TEXT_TITLE

            r2 = p.add_run()
            r2.text = n_txt
            r2.font.size = Pt(10.5)
            r2.font.color.rgb = TEXT_MUTED
            p.space_after = Pt(8)

    # ==========================================
    # SLIDE 7: TỐI ƯU HIỆU NĂNG - OBJECT POOLING
    # ==========================================
    s7 = prs.slides.add_slide(blank_layout)
    set_slide_background(s7)
    add_header(s7, "Tối ưu hóa hiệu năng", "TỐI ƯU HIỆU NĂNG VỚI KỸ THUẬT OBJECT POOLING", "Triệt tiêu hiện tượng nghẽn bộ nhớ và sụt giảm khung hình, duy trì 60+ FPS ổn định")

    # Left: Without Pooling
    create_card(s7, Inches(0.8), Inches(1.8), Inches(5.75), Inches(3.7), ACCENT_RED, BG_CARD)
    tb_w1 = s7.shapes.add_textbox(Inches(1.05), Inches(1.95), Inches(5.25), Inches(0.45))
    p = tb_w1.text_frame.paragraphs[0]
    p.text = "❌ CÁCH TRUYỀN THỐNG (KHÔNG DÙNG POOL)"
    p.font.bold = True
    p.font.size = Pt(13)
    p.font.color.rgb = ACCENT_RED

    tb_b1 = s7.shapes.add_textbox(Inches(1.05), Inches(2.45), Inches(5.25), Inches(2.8))
    tf_b1 = tb_b1.text_frame
    tf_b1.word_wrap = True
    bad_pts = [
        ("Liên tục Instantiate / Destroy:", "Mỗi quả sinh ra và biến mất liên tục tạo ra các vùng nhớ rác mới."),
        ("Phân mảnh RAM nghiêm trọng:", "Hệ điều hành và Unity Engine phải cấp phát và thu hồi RAM liên tục."),
        ("Garbage Collector Spike:", "Bộ dọn rác GC bất ngờ dừng luồng game để giải phóng RAM."),
        ("Hậu quả trực tiếp:", "Sụt FPS, hình ảnh bị khựng giật (Stuttering), trải nghiệm chém bị trễ.")
    ]
    for idx, (b_txt, n_txt) in enumerate(bad_pts):
        p = tf_b1.paragraphs[0] if idx == 0 else tf_b1.add_paragraph()
        r1 = p.add_run()
        r1.text = f"✖ {b_txt} "
        r1.font.bold = True
        r1.font.size = Pt(11)
        r1.font.color.rgb = TEXT_TITLE
        r2 = p.add_run()
        r2.text = n_txt
        r2.font.size = Pt(10.5)
        r2.font.color.rgb = TEXT_MUTED
        p.space_after = Pt(4)

    # Right: With Object Pooling
    create_card(s7, Inches(6.78), Inches(1.8), Inches(5.75), Inches(3.7), ACCENT_GREEN, BG_CARD)
    tb_w2 = s7.shapes.add_textbox(Inches(7.03), Inches(1.95), Inches(5.25), Inches(0.45))
    p = tb_w2.text_frame.paragraphs[0]
    p.text = "✔ GIẢI PHÁP OBJECT POOLING (ÁP DỤNG)"
    p.font.bold = True
    p.font.size = Pt(13)
    p.font.color.rgb = ACCENT_GREEN

    tb_b2 = s7.shapes.add_textbox(Inches(7.03), Inches(2.45), Inches(5.25), Inches(2.8))
    tf_b2 = tb_b2.text_frame
    tf_b2.word_wrap = True
    good_pts = [
        ("Khởi tạo sẵn một lần:", "Tạo trước 1 kho đối tượng quả và bom ngay khi nạp Scene đầu game."),
        ("Tái chế tức thời (Recycle):", "Bật `SetActive(true)` khi bắn quả, tắt `SetActive(false)` khi chém xong."),
        ("Không sinh rác bộ nhớ:", "Số lượng Object trong RAM giữ nguyên, GC Allocation đạt ~0 byte."),
        ("Kết quả đạt được:", "Duy trì ổn định 60+ FPS, gameplay mượt mà trên mọi dòng máy tính.")
    ]
    for idx, (b_txt, n_txt) in enumerate(good_pts):
        p = tf_b2.paragraphs[0] if idx == 0 else tf_b2.add_paragraph()
        r1 = p.add_run()
        r1.text = f"★ {b_txt} "
        r1.font.bold = True
        r1.font.size = Pt(11)
        r1.font.color.rgb = TEXT_TITLE
        r2 = p.add_run()
        r2.text = n_txt
        r2.font.size = Pt(10.5)
        r2.font.color.rgb = TEXT_MUTED
        p.space_after = Pt(4)

    # Bottom 3 Stat Metric Badges
    stats = [
        ("60+ FPS", "Tốc độ khung hình ổn định", ACCENT_GREEN),
        ("~0 ms", "Thời gian dừng chờ Garbage Collector", TEXT_GOLD),
        ("-40% CPU", "Mức tiêu hao tài nguyên xử lý", ACCENT_ORANGE)
    ]
    for idx, (stat_num, stat_desc, stat_col) in enumerate(stats):
        bx = Inches(0.8 + idx * 4.04)
        c_stat = create_card(s7, bx, Inches(5.7), Inches(3.64), Inches(1.15), stat_col, BG_CARD_LIGHT)

        tb_s = s7.shapes.add_textbox(bx + Inches(0.1), Inches(5.78), Inches(3.44), Inches(0.95))
        tf_s = tb_s.text_frame
        p1 = tf_s.paragraphs[0]
        p1.alignment = PP_ALIGN.CENTER
        p1.text = stat_num
        p1.font.bold = True
        p1.font.size = Pt(20)
        p1.font.color.rgb = stat_col

        p2 = tf_s.add_paragraph()
        p2.alignment = PP_ALIGN.CENTER
        p2.text = stat_desc
        p2.font.size = Pt(10.5)
        p2.font.color.rgb = TEXT_BODY

    # ==========================================
    # SLIDE 8: HỆ THỐNG ĐIỂM SỐ, COMBO & KỶ LỤC
    # ==========================================
    s8 = prs.slides.add_slide(blank_layout)
    set_slide_background(s8)
    add_header(s8, "Cơ chế tính điểm", "HỆ THỐNG ĐIỂM SỐ, COMBO STREAK & KỶ LỤC", "Tạo động lực cạnh tranh, tăng tính lôi cuốn và thúc đẩy giá trị chơi lại nhiều lần")

    score_blocks = [
        ("01", "ĐIỂM SỐ & COMBO STREAK", TEXT_GOLD, [
            ("Điểm chém cơ bản:", "Mỗi quả hoa quả chém trúng cộng trực tiếp +1 Điểm."),
            ("Cơ chế Combo Streak:", "Chém trúng từ 3 quả trở lên trong 1 đường quẹt chuột."),
            ("Cấp số nhân điểm thưởng:", "Điểm nhận được = Số quả x Hệ số Combo thưởng."),
            ("Hiệu ứng thỏa mãn:", "Hiện popup chữ COMBO nổi bật khích lệ người chơi gom quả.")
        ]),
        ("02", "3 TIM SINH MỆNH & BẪY BOM", ACCENT_RED, [
            ("Khởi đầu với 3 Tim:", "Hiển thị 3 biểu tượng Trái Tim ở góc trên màn hình."),
            ("Cơ chế tha bổng:", "Quả rơi tự do chạm đáy màn hình không bị trừ mạng."),
            ("Trừng phạt nghiêm khắc:", "Chém trúng bom nổ trừ ngay 1 Tim và rung giật màn hình."),
            ("Kết thúc trò chơi:", "Mất hết 3 Tim trò chơi lập tức chuyển sang Game Over.")
        ]),
        ("03", "LƯU KỶ LỤC (HIGH SCORE)", ACCENT_GREEN, [
            ("Lưu trữ bền vững:", "Sử dụng công nghệ PlayerPrefs của Unity Engine."),
            ("Tự so sánh thành tích:", "Tự động so sánh điểm số sau mỗi ván chơi với kỷ lục cũ."),
            ("Vinh danh kỷ lục mới:", "Hiển thị nhãn 'NEW BEST SCORE!' khi phá kỷ lục."),
            ("Dữ liệu vĩnh viễn:", "Điểm kỷ lục được lưu giữ ngay cả khi tắt game và mở lại.")
        ])
    ]

    for idx, (num, title, col, pts) in enumerate(score_blocks):
        x = Inches(0.8 + idx * 4.04)
        create_card(s8, x, Inches(1.8), Inches(3.64), Inches(5.1), col, BG_CARD)

        hb = s8.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, x + Inches(0.15), Inches(1.95), Inches(3.34), Inches(0.8))
        hb.fill.solid()
        hb.fill.fore_color.rgb = BG_CARD_LIGHT
        hb.line.color.rgb = col
        hb.line.width = Pt(1)

        tb_h = s8.shapes.add_textbox(x + Inches(0.2), Inches(2.0), Inches(3.24), Inches(0.7))
        p = tb_h.text_frame.paragraphs[0]
        p.text = f"{num} • {title}"
        p.font.bold = True
        p.font.size = Pt(11.5)
        p.font.color.rgb = col

        tb_b = s8.shapes.add_textbox(x + Inches(0.18), Inches(2.9), Inches(3.28), Inches(3.8))
        tf_b = tb_b.text_frame
        tf_b.word_wrap = True
        for p_i, (b_txt, n_txt) in enumerate(pts):
            p = tf_b.paragraphs[0] if p_i == 0 else tf_b.add_paragraph()
            r1 = p.add_run()
            r1.text = f"• {b_txt} "
            r1.font.bold = True
            r1.font.size = Pt(11)
            r1.font.color.rgb = TEXT_TITLE

            r2 = p.add_run()
            r2.text = n_txt
            r2.font.size = Pt(10.5)
            r2.font.color.rgb = TEXT_MUTED
            p.space_after = Pt(8)

    # ==========================================
    # SLIDE 9: THIẾT KẾ GIAO DIỆN & TRẢI NGHIỆM VÕ QUÁN
    # ==========================================
    s9 = prs.slides.add_slide(blank_layout)
    set_slide_background(s9)
    add_header(s9, "Mỹ thuật & Giao diện", "THIẾT KẾ GIAO DIỆN & TRẢI NGHIỆM VÕ QUÁN", "Chủ đề võ quán kiếm đạo mộc mạc, tối giản giao diện để tối đa hóa không gian chém quả")

    ui_screens = [
        ("MÀN HÌNH CHÍNH (MAIN MENU)", TEXT_GOLD, [
            ("Không gian Võ quán:", "Nền thớt gỗ ấm áp, phong cách kiếm đạo truyền thống."),
            ("Nút Play phong cách bảng gỗ:", "Kích thước to rõ, dễ tương tác trên cả PC và cảm ứng."),
            ("Hiển thị High Score:", "Điểm kỷ lục hiển thị ngay sảnh chờ khích lệ tinh thần thi đấu.")
        ]),
        ("GIAO DIỆN TRONG GAME (HUD)", ACCENT_ORANGE, [
            ("Thiết kế tối giản (Minimal):", "Chỉ hiển thị Điểm số hiện tại và 3 Tim sinh mệnh."),
            ("Góc nhìn thông thoáng 90%:", "HUD gom gọn ở cạnh trên, để trọn không gian cho đường kiếm."),
            ("Hiệu ứng vệt chém rực sáng:", "Vệt kiếm bám sát chuột tạo cảm giác sắc bén, chân thực.")
        ]),
        ("GAME OVER & THỐNG KÊ", ACCENT_GREEN, [
            ("Bảng tổng kết chiến tích:", "Hiển thị điểm số vừa đạt được và so sánh với kỷ lục cũ."),
            ("Huy hiệu New Record:", "Vinh danh thành tích khi người chơi phá vỡ mốc điểm cao nhất."),
            ("Tái đấu tức thì:", "Nút Restart nạp lại trận đấu trong 0.1 giây không độ trễ.")
        ])
    ]

    for idx, (title, col, pts) in enumerate(ui_screens):
        x = Inches(0.8 + idx * 4.04)
        create_card(s9, x, Inches(1.8), Inches(3.64), Inches(5.1), col, BG_CARD)

        hb = s9.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, x + Inches(0.15), Inches(1.95), Inches(3.34), Inches(0.8))
        hb.fill.solid()
        hb.fill.fore_color.rgb = BG_CARD_LIGHT
        hb.line.color.rgb = col
        hb.line.width = Pt(1)

        tb_h = s9.shapes.add_textbox(x + Inches(0.2), Inches(2.0), Inches(3.24), Inches(0.7))
        p = tb_h.text_frame.paragraphs[0]
        p.text = title
        p.font.bold = True
        p.font.size = Pt(11.5)
        p.font.color.rgb = col

        tb_b = s9.shapes.add_textbox(x + Inches(0.18), Inches(2.9), Inches(3.28), Inches(3.8))
        tf_b = tb_b.text_frame
        tf_b.word_wrap = True
        for p_i, (b_txt, n_txt) in enumerate(pts):
            p = tf_b.paragraphs[0] if p_i == 0 else tf_b.add_paragraph()
            r1 = p.add_run()
            r1.text = f"★ {b_txt} "
            r1.font.bold = True
            r1.font.size = Pt(11)
            r1.font.color.rgb = TEXT_TITLE

            r2 = p.add_run()
            r2.text = n_txt
            r2.font.size = Pt(10.5)
            r2.font.color.rgb = TEXT_MUTED
            p.space_after = Pt(10)

    # ==========================================
    # SLIDE 10: KẾT LUẬN & HƯỚNG PHÁT TRIỂN
    # ==========================================
    s10 = prs.slides.add_slide(blank_layout)
    set_slide_background(s10)
    add_header(s10, "Tổng kết đồ án", "KẾT LUẬN & HƯỚNG PHÁT TRIỂN DỰ ÁN", "Đánh giá mức độ hoàn thiện sản phẩm và lộ trình nâng cấp mở rộng trong tương lai")

    # Left: Results Achieved
    create_card(s10, Inches(0.8), Inches(1.8), Inches(5.75), Inches(3.5), TEXT_GOLD, BG_CARD)
    tb_rt = s10.shapes.add_textbox(Inches(1.05), Inches(1.95), Inches(5.25), Inches(0.45))
    p = tb_rt.text_frame.paragraphs[0]
    p.text = "🏆 KẾT QUẢ ĐẠT ĐƯỢC CỦA ĐỒ ÁN"
    p.font.bold = True
    p.font.size = Pt(13)
    p.font.color.rgb = TEXT_GOLD

    tb_rb = s10.shapes.add_textbox(Inches(1.05), Inches(2.45), Inches(5.25), Inches(2.7))
    tf_rb = tb_rb.text_frame
    tf_rb.word_wrap = True
    res_pts = [
        ("Hoàn thiện 100% Core Gameplay:", "Cơ chế chém quả, nảy vật lý Parabol, bẫy bom vận hành chính xác."),
        ("Hiệu năng tối ưu tuyệt đối:", "Áp dụng Object Pooling đạt 60+ FPS ổn định, triệt tiêu giật lag."),
        ("Kiến trúc mã nguồn chuẩn:", "Tuân thủ OOP, Singleton Pattern, phân tách trách nhiệm các Manager rõ ràng."),
        ("Đồ họa & Âm thanh sống động:", "Phong cách võ quán ấm cúng, hiệu ứng nước ép và rung camera cuốn hút.")
    ]
    for idx, (b_txt, n_txt) in enumerate(res_pts):
        p = tf_rb.paragraphs[0] if idx == 0 else tf_rb.add_paragraph()
        r1 = p.add_run()
        r1.text = f"✔ {b_txt} "
        r1.font.bold = True
        r1.font.size = Pt(11)
        r1.font.color.rgb = TEXT_TITLE

        r2 = p.add_run()
        r2.text = n_txt
        r2.font.size = Pt(10.5)
        r2.font.color.rgb = TEXT_MUTED
        p.space_after = Pt(4)

    # Right: Future Roadmap
    create_card(s10, Inches(6.78), Inches(1.8), Inches(5.75), Inches(3.5), ACCENT_ORANGE, BG_CARD)
    tb_ft = s10.shapes.add_textbox(Inches(7.03), Inches(1.95), Inches(5.25), Inches(0.45))
    p = tb_ft.text_frame.paragraphs[0]
    p.text = "🚀 HƯỚNG PHÁT TRIỂN TRONG TƯƠNG LAI"
    p.font.bold = True
    p.font.size = Pt(13)
    p.font.color.rgb = ACCENT_ORANGE

    tb_fb = s10.shapes.add_textbox(Inches(7.03), Inches(2.45), Inches(5.25), Inches(2.7))
    tf_fb = tb_fb.text_frame
    tf_fb.word_wrap = True
    fut_pts = [
        ("Chế độ chơi mới:", "Bổ sung Zen Mode (90s thư giãn không bom) và Arcade Mode tính giờ."),
        ("Trái cây kỹ năng đặc biệt:", "Chuối Freeze (đóng băng thời gian), Chuối Frenzy (bão quả liên tục)."),
        ("Tùy biến Kiếm & Võ quán:", "Cửa hàng Skin kiếm (lửa, băng, lôi) và đổi phông nền võ đường."),
        ("Bảng xếp hạng Online:", "Tích hợp Leaderboard trực tuyến để người chơi tranh tài toàn cầu.")
    ]
    for idx, (b_txt, n_txt) in enumerate(fut_pts):
        p = tf_fb.paragraphs[0] if idx == 0 else tf_fb.add_paragraph()
        r1 = p.add_run()
        r1.text = f"✦ {b_txt} "
        r1.font.bold = True
        r1.font.size = Pt(11)
        r1.font.color.rgb = TEXT_TITLE

        r2 = p.add_run()
        r2.text = n_txt
        r2.font.size = Pt(10.5)
        r2.font.color.rgb = TEXT_MUTED
        p.space_after = Pt(4)

    # Bottom Banner: Thank You & Q&A
    create_card(s10, Inches(0.8), Inches(5.48), Inches(11.733), Inches(1.45), BORDER_GOLD, BG_CARD_LIGHT)
    tb_thx = s10.shapes.add_textbox(Inches(1.0), Inches(5.58), Inches(11.333), Inches(1.25))
    tf_thx = tb_thx.text_frame
    p1 = tf_thx.paragraphs[0]
    p1.alignment = PP_ALIGN.CENTER
    p1.text = "CHÂN THÀNH CẢM ƠN THẦY CÔ VÀ HỘI ĐỒNG ĐÃ LẮNG NGHE!"
    p1.font.bold = True
    p1.font.size = Pt(18)
    p1.font.color.rgb = TEXT_GOLD

    p2 = tf_thx.add_paragraph()
    p2.alignment = PP_ALIGN.CENTER
    p2.text = "Kính mời Quý Thầy Cô và các bạn đặt câu hỏi đóng góp cho đồ án (Q&A)"
    p2.font.size = Pt(13)
    p2.font.color.rgb = TEXT_TITLE

    # Save to both primary requested file and existing presentation file
    out1 = os.path.join(BASE_DIR, "FRUIT_NINJA_GAME_2D.pptx")
    out2 = os.path.join(BASE_DIR, "FRUIT_NINJA_2D.pptx")
    prs.save(out1)
    prs.save(out2)
    print(f"Presentations successfully saved to:\n  {out1}\n  {out2}")

if __name__ == "__main__":
    build_presentation()
