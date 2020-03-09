using System;
namespace Ringer.Types
{
    [Flags]
    public enum MessageTypes
    {
        ///// <summary>
        ///// HasFlag(MessageTypes.None) always TRUE
        ///// <see ref="https://stackoverflow.com/a/15436676"/>
        ///// </summary>
        None = 0b_0000_0000_0000,
        /// <summary>
        /// 왼쪽 정렬
        /// </summary>
        Incomming = 0b_0000_0000_0001,
        /// <summary>
        /// 오른쪽 정렬
        /// </summary>
        Outgoing = 0b_0000_0000_0010,
        /// <summary>
        /// 분 단위 섹션의 시작. sender 이름이 표시된다.
        /// </summary>
        Leading = 0b_0000_0000_0100,
        /// <summary>
        /// 분 단위 섹션의 끝. 타임스탬프가 표시된다.
        /// </summary>
        Trailing = 0b_0000_0000_1000,
        /// <summary>
        /// 탭하면 이미지 보기
        /// </summary>
        Image = 0b_0000_0001_0000,
        /// <summary>
        /// 탭하면 비디오 보기
        /// </summary>
        Video = 0b_0000_0010_0000,
        /// <summary>
        /// 유저의 출입을 표시. 가운데 정렬
        /// </summary>
        Text = 0b_0000_0100_0000,
        /// <summary>
        /// 유저의 출입을 표시. 가운데 정렬
        /// </summary>
        EntranceNotice = 0b_0000_1000_0000,
        /// <summary>
        /// 새 날짜의 시작을 표시. 가운데 정렬
        /// </summary>
        DateNotice = 0b_0001_0000_0000
    }
}
