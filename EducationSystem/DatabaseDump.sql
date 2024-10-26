
CREATE DATABASE corporate_training;

CREATE TABLE users (

                       user_id SERIAL PRIMARY KEY,

                       first_name VARCHAR(50) NOT NULL,

                       last_name VARCHAR(50) NOT NULL,

                       email VARCHAR(100) UNIQUE NOT NULL,

                       password_hash VARCHAR(100) NOT NULL,

                       role VARCHAR(20) NOT NULL CHECK (role IN ('Administrator', 'Instructor', 'Participant')),

                       created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

                       updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

                       phone_number VARCHAR(20),

                       department VARCHAR(50),

                       salt bytea not null

);

CREATE TABLE courses (

                         course_id SERIAL PRIMARY KEY,

                         title VARCHAR(100) NOT NULL,

                         description TEXT,

                         duration INT NOT NULL CHECK (duration > 0),

                         created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

                         updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

                         instructor_id INT REFERENCES users(user_id)

);

CREATE TABLE enrollments (

                             enrollment_id SERIAL PRIMARY KEY,

                             user_id INT REFERENCES users(user_id),

                             course_id INT REFERENCES courses(course_id),

                             enrollment_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

                             completion_date TIMESTAMP,

                             grade DECIMAL(5,2) CHECK (grade >= 2 AND grade <= 5)

);

CREATE TABLE reports (

                         report_id SERIAL PRIMARY KEY,

                         course_id INT REFERENCES courses(course_id),

                         total_hours DECIMAL(8,2),

                         avg_grade DECIMAL(5,2),

                         completion_rate DECIMAL(5,2) CHECK (completion_rate >= 0 AND completion_rate <= 100),

                         generated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP

);

create table materials
(
    material_id serial primary key,
    material_name text not null,
    course_id integer not null references courses(course_id)
);

