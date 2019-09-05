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
  `user_id` VARCHAR(45) NULL DEFAULT NULL,
  `item_code` VARCHAR(20) NULL
);


select * from taobao_goods WHERE prd_category = 'D00020201';

select * from taobao_goods g
LEFT OUTER JOIN taobao_category c
ON g.prd_category = c.Id
WHERE prd_category = '1';


SELECT G.id as id, G.prd_img as prd_img , C.cate_name as prd_category, G.prd_name as prd_name, G.prd_attr as prd_attr, G.detail_yn as detail_yn, G.prd_price as prd_price
, G.prd_promo as prd_promo, G.prd_brand as prd_brand, G.opt_1 as opt_1, G.opt_val_1 as opt_val_1, G.opt_2 as opt_2, G.opt_val_2 as opt_val_2, G.opt_3 as opt_3, G.opt_val_3 as opt_val_3
, G.prd_opt_imgs as prd_opt_imgs, G.prd_stock as prd_stock, G.detail_img as detail_img, G.add_img_1 as add_img_1, G.add_img_2 as add_img_2, G.add_img_3 as add_img_3, G.add_img_4 as add_img_4
, G.created_date as created_date, G.updated_date as updated_date, G.user_id as user_id, C.Id as cate_id FROM taobao_goods G LEFT OUTER JOIN taobao_category C ON G.prd_category = C.Id 
WHERE prd_status = '1' AND SUBSTR(prd_category, 1, 6) = '000101'  ;