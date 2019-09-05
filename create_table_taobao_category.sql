CREATE TABLE taobao_category (
	`id` int auto_increment primary key,
	`cate_name` VARCHAR(200) NOT NULL,
	`cate_desc` VARCHAR(4000) NULL
);

SELECT * FROM taobao_category;
SELECT count(*) as cnt FROM taobao_category WHERE cate_name = '테스트2';

UPDATE taobao_category SET cate_name = 'tesst', cate_desc = 'asdfxcvcxz' WHERE id = 1;


CREATE TABLE category(
	`id` int auto_increment primary key,
    `cate_name` VARCHAR(200) NOT NULL,
    `cate_type` VARCHAR(1) NOT NULL,
    `L` VARCHAR(2) NOT NULL,
    `M` VARCHAR(2) NULL default '00',
    `S` VARCHAR(2) NULL default '00',
    `XS` VARCHAR(3) NULL default '00',
    `CODE` VARCHAR(20) NULL
);

DROP TABLE category;
INSERT INTO category(cate_name, cate_type, L) VALUE('신발', 'A', '01');

SELECT * FROM category 
/*WHERE L = '01' AND M = '03' AND cate_type = 'C'*/
order by cate_type, L, M, S, XS;
SELECT * FROM category WHERE M = "01";
SELECT MAX(L) as L FROM category;
SELECT * FROM category WHERE cate_name in ('의류', '신발') and cate_type = 'A';
SELECT count(*) as cnt, L FROM category WHERE cate_name = '의류1' AND cate_type = 'A';

SELECT count(*) as cnt, L FROM category WHERE cate_name = '악세사리' AND cate_type = 'A';

UPDATE category SET S = '03', XS = '00' WHERE id = 13;

UPDATE category SET L = '01', M = '03', S = '02', XS = '01', CODE = '123' WHERE id = 11;
