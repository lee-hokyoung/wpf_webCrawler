drop table taobao_goods;

CREATE TABLE `taobao_goods` (
  `id` VARCHAR(20) NOT NULL,
  `prd_img` VARCHAR(200) NULL DEFAULT NULL,
  `prd_category` VARCHAR(45) NULL DEFAULT NULL,
  `prd_name` VARCHAR(100) NOT NULL,
  `prd_attr` VARCHAR(2000) NULL DEFAULT NULL,
  `detail_yn` VARCHAR(1) NULL DEFAULT '0',
  `prd_price` VARCHAR(50) NULL DEFAULT NULL,
  `prd_promo` VARCHAR(50) NULL DEFAULT NULL,
  `prd_opt` VARCHAR(800) NULL DEFAULT NULL,
  `prd_brand` VARCHAR(100) NULL DEFAULT NULL,
  `opt_1` VARCHAR(45) NULL DEFAULT NULL,
  `opt_val_1` VARCHAR(500) NULL DEFAULT NULL,
  `opt_2` VARCHAR(45) NULL DEFAULT NULL,
  `opt_val_2` VARCHAR(500) NULL DEFAULT NULL,
  `opt_3` VARCHAR(45) NULL DEFAULT NULL,
  `opt_val_3` VARCHAR(500) NULL DEFAULT NULL,
  `prd_status` VARCHAR(1) NULL DEFAULT '1' COMMENT '1 : 판매중\\\\\\\\n8 : 재고없음\\\\\\\\n9 : 제외품목',
  `prd_stock` VARCHAR(45) NULL DEFAULT NULL,
  `prd_opt_imgs` TEXT NULL DEFAULT NULL,
  `detail_img` TEXT NULL DEFAULT NULL,
  `add_img_1` VARCHAR(200) NULL DEFAULT NULL,
  `add_img_2` VARCHAR(200) NULL DEFAULT NULL,
  `add_img_3` VARCHAR(200) NULL DEFAULT NULL,
  `add_img_4` VARCHAR(200) NULL DEFAULT NULL,
  `excel_down_yn` VARCHAR(1) NULL DEFAULT NULL,	
  `created_date` VARCHAR(45) NULL DEFAULT NULL,
  `updated_date` VARCHAR(45) NULL DEFAULT NULL,
  `user_id` VARCHAR(45) NULL DEFAULT NULL
);


select * from taobao_goods
WHERE substr(created_date, 1, 10) = '2019-08-31';

select * from taobao_goods g
LEFT OUTER JOIN taobao_category c
ON g.prd_category = c.Id
WHERE prd_category = '1';