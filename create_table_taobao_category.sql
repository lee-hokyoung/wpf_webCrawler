CREATE TABLE taobao_category (
	`id` int auto_increment primary key,
	`cate_name` VARCHAR(200) NOT NULL,
	`cate_desc` VARCHAR(4000) NULL
);

SELECT * FROM taobao_category;
SELECT count(*) as cnt FROM taobao_category WHERE cate_name = '테스트2';

UPDATE taobao_category SET cate_name = 'tesst', cate_desc = 'asdfxcvcxz' WHERE id = 1;