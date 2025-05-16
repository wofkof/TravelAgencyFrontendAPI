// In file: DTOs/OrderDTOs/OrderParticipantDto.cs
using System;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models; // For GenderType, DocumentType enums

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderParticipantDto
    {
        // �p�G�ȫȬO�q�u�`�ήȫȲM��v������A�i�H�ǤJ MemberFavoriteTravelerId
        // ��ݥi�H�ھڳo��ID�w�񳡤���ơA���������\�e���л\/���ѳ̷s���
        public int? FavoriteTravelerId { get; set; } // ���� MemberFavoriteTraveler.Id

        // �p�G�o��ȫȴN�O�q�ʤH(��e�n�J�|��)�A�i�H�Τ@�Ӽлx�����e�ݪ�����J�|��ID
        // �t�@�ؤ覡�O�e�ݶǰe�Ӯȫȹ����� MemberId (�p�G�O�t�Τ����|��)
        public int? MemberIdAsParticipant { get; set; } // �Y���ȫȥ����]�O�t�η|���A�h�� MemberId

        [Required(ErrorMessage = "�ȫȩm�W������")]
        [StringLength(100)]
        public string Name { get; set; }

        // BirthDate, IdNumber, Gender �����ھ� OrderParticipant Model �[�J
        // �o�ǬO��ڮȫȸ�ơA�Y�ϱq�`�ήȫȱa�J�A�]���]�t�b�e����ݪ���Ƥ�
        [Required(ErrorMessage = "�ȫȥͤ鬰����")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "�ȫȨ����Ҹ�/�ҥ󸹽X������")]
        [StringLength(50)]
        public string IdNumber { get; set; } // �ھ� DocumentType �i��������P�ҥ󸹽X

        [Required(ErrorMessage = "�ȫȩʧO������")]
        public GenderType Gender { get; set; }

        //[Required(ErrorMessage = "�ȫȤ��������")]
        //[Phone(ErrorMessage = "�п�J���Ī�������X")]
        [StringLength(20)]
        public string Phone { get; set; }

        //[Required(ErrorMessage = "�ȫȹq�l�H�c������")]
        //[EmailAddress(ErrorMessage = "�п�J���Ī��q�l�H�c")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required(ErrorMessage = "�ȫ��ҥ�����������")]
        public DocumentType DocumentType { get; set; } // Passport, ResidencePermit, EntryPermit

        [StringLength(100)]
        public string? DocumentNumber { get; set; } // ��ڪ��ҥ󸹽X (�Y�PIdNumber���P�Χ����)

        // �ھ� DocumentType == Passport �ɪ�������� (�i��)
        [StringLength(100)]
        public string? PassportSurname { get; set; }
        [StringLength(100)]
        public string? PassportGivenName { get; set; }
        public DateTime? PassportExpireDate { get; set; }
        [StringLength(100)]
        public string? Nationality { get; set; }

        [StringLength(255)]
        public string? Note { get; set; } // �ӧO�ȫȪ��Ƶ�
    }
}